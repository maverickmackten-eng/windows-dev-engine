"""Headless tests for the bridge: run with  python -m unittest discover tests"""
import os
import socket
import sys
import time
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

from bridge import Bridge                      # noqa: E402
from protocol import LineReader, encode        # noqa: E402


class Peer:
    def __init__(self, port, name, role="terminal"):
        self.sock = socket.create_connection(("127.0.0.1", port), timeout=3)
        self.reader = LineReader(self.sock)
        self.name = name
        self.send({"type": "hello", "name": name, "role": role})
        self.welcome = self.read()

    def send(self, obj):
        self.sock.sendall(encode(obj))

    def read(self):
        return self.reader.read()

    def read_until(self, mtype, pred=lambda m: True, tries=20):
        for _ in range(tries):
            m = self.read()
            if m is None:
                return None
            if m["type"] == mtype and pred(m):
                return m
        raise AssertionError(f"no matching {mtype} received")

    def close(self):
        self.sock.close()


class BridgeTests(unittest.TestCase):
    def setUp(self):
        self.bridge = Bridge(port=0).start()
        self.port = self.bridge.port
        self.peers = []

    def tearDown(self):
        for p in self.peers:
            p.close()
        self.bridge.stop()

    def peer(self, name, role="terminal"):
        p = Peer(self.port, name, role)
        self.peers.append(p)
        return p

    def test_hello_and_welcome(self):
        a = self.peer("Alice")
        self.assertEqual(a.welcome["type"], "welcome")
        self.assertEqual(a.welcome["name"], "Alice")
        self.assertEqual([c["name"] for c in a.welcome["contacts"]], ["Alice"])

    def test_duplicate_name_rejected(self):
        self.peer("Alice")
        p = self.peer("Alice")
        self.assertEqual(p.welcome["type"], "error")
        self.assertIn("already online", p.welcome["text"])

    def test_bad_name_rejected(self):
        p = self.peer("bad name!")
        self.assertEqual(p.welcome["type"], "error")

    def test_broadcast_reaches_everyone_and_board(self):
        a, b, board = self.peer("Alice"), self.peer("Bob"), self.peer("Board", "board")
        a.send({"type": "send", "to": "*", "text": "hi all"})
        for p in (a, b, board):
            m = p.read_until("message")
            self.assertEqual((m["from"], m["to"], m["text"]), ("Alice", "*", "hi all"))

    def test_direct_message_is_private(self):
        a, b, c, board = self.peer("Alice"), self.peer("Bob"), self.peer("Carol"), self.peer("Board", "board")
        a.send({"type": "send", "to": "Bob", "text": "secret"})
        self.assertEqual(b.read_until("message")["text"], "secret")
        self.assertEqual(a.read_until("message")["text"], "secret")   # echo to sender
        self.assertEqual(board.read_until("message")["text"], "secret")
        # Carol must not get it: send a broadcast after and check it's the first message she sees
        a.send({"type": "send", "to": "*", "text": "public"})
        self.assertEqual(c.read_until("message")["text"], "public")

    def test_unknown_target_errors(self):
        a = self.peer("Alice")
        a.send({"type": "send", "to": "Nobody", "text": "x"})
        self.assertIn("unknown terminal", a.read_until("error")["text"])

    def test_group_create_and_message(self):
        a, b, c = self.peer("Alice"), self.peer("Bob"), self.peer("Carol")
        a.send({"type": "group_create", "name": "dev", "members": ["Bob"]})
        pres = a.read_until("presence", lambda m: "dev" in m["groups"])
        self.assertEqual(pres["groups"]["dev"], ["Alice", "Bob"])
        a.send({"type": "send", "to": "#dev", "text": "group hi"})
        self.assertEqual(b.read_until("message")["to"], "#dev")
        # Carol is not a member and cannot post
        c.send({"type": "send", "to": "#dev", "text": "intruder"})
        self.assertIn("not a member", c.read_until("error")["text"])
        # Carol joins and then can post
        c.send({"type": "group_join", "name": "dev"})
        c.send({"type": "send", "to": "#dev", "text": "now in"})
        self.assertEqual(c.read_until("message")["text"], "now in")

    def test_history_replays_to_late_joiner(self):
        a = self.peer("Alice")
        a.send({"type": "send", "to": "*", "text": "first"})
        a.read_until("message")
        b = self.peer("Bob")
        self.assertEqual([m["text"] for m in b.welcome["history"]], ["first"])

    def test_offline_contact_stays_listed(self):
        a = self.peer("Alice")
        b = self.peer("Bob")
        b.close()
        pres = a.read_until("presence",
                            lambda m: any(c["name"] == "Bob" and not c["online"] for c in m["contacts"]))
        bob = [c for c in pres["contacts"] if c["name"] == "Bob"][0]
        self.assertFalse(bob["online"])

    def test_malformed_line_does_not_crash_server(self):
        a = self.peer("Alice")
        raw = socket.create_connection(("127.0.0.1", self.port), timeout=3)
        raw.sendall(b"this is not json\n")
        time.sleep(0.2)
        raw.close()
        a.send({"type": "send", "to": "*", "text": "still alive"})
        self.assertEqual(a.read_until("message")["text"], "still alive")

    def test_message_before_hello_rejected(self):
        raw = socket.create_connection(("127.0.0.1", self.port), timeout=3)
        raw.sendall(encode({"type": "send", "to": "*", "text": "x"}))
        self.assertEqual(LineReader(raw).read()["type"], "error")
        raw.close()


if __name__ == "__main__":
    unittest.main()
