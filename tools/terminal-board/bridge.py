"""Bridge server: the shared message hub every terminal and the board connect to.

Run:  python bridge.py [--port 7788] [--log messages.jsonl]

Routing rules
    to == "*"        -> everyone (group chat)
    to == "<name>"   -> that terminal + the sender (direct message)
    to == "#<group>" -> members of that group
The board window (role "board") always receives every message so it can show
all conversations.
"""
import argparse
import json
import socket
import threading
import time

from protocol import (BROADCAST, DEFAULT_HOST, DEFAULT_PORT, GROUP_PREFIX,
                      LineReader, encode, valid_name)


class Client:
    def __init__(self, sock, addr):
        self.sock = sock
        self.addr = addr
        self.name = None
        self.role = "terminal"
        self.lock = threading.Lock()

    def send(self, obj):
        try:
            with self.lock:
                self.sock.sendall(encode(obj))
        except OSError:
            pass


class Bridge:
    def __init__(self, host=DEFAULT_HOST, port=DEFAULT_PORT, log_path=None, history_limit=2000):
        self.host = host
        self.port = port
        self.log_path = log_path
        self.history_limit = history_limit
        self.lock = threading.RLock()
        self.clients = []                 # connected Client objects
        self.known = {}                   # name -> last-seen timestamp (all names ever seen)
        self.groups = {}                  # group name -> set of member names
        self.history = []                 # list of message dicts
        self.next_id = 1
        self.server = None
        self._stop = threading.Event()

    # ----- lifecycle -------------------------------------------------------
    def start(self):
        self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.server.bind((self.host, self.port))
        self.server.listen(32)
        self.port = self.server.getsockname()[1]
        threading.Thread(target=self._accept_loop, daemon=True).start()
        return self

    def stop(self):
        self._stop.set()
        try:
            self.server.close()
        except OSError:
            pass
        with self.lock:
            for c in list(self.clients):
                try:
                    c.sock.close()
                except OSError:
                    pass

    def serve_forever(self):
        try:
            while not self._stop.is_set():
                time.sleep(0.5)
        except KeyboardInterrupt:
            pass
        finally:
            self.stop()

    def _accept_loop(self):
        while not self._stop.is_set():
            try:
                sock, addr = self.server.accept()
            except OSError:
                return
            client = Client(sock, addr)
            threading.Thread(target=self._client_loop, args=(client,), daemon=True).start()

    # ----- helpers ---------------------------------------------------------
    def _online_names(self):
        return {c.name for c in self.clients if c.name and c.role == "terminal"}

    def _presence_payload(self):
        online = self._online_names()
        contacts = [{"name": n, "online": n in online} for n in sorted(self.known)]
        groups = {g: sorted(m) for g, m in self.groups.items()}
        return {"type": "presence", "contacts": contacts, "groups": groups}

    def _broadcast_presence(self):
        payload = self._presence_payload()
        for c in self.clients:
            if c.name:
                c.send(payload)

    def _system(self, text, exclude=None):
        for c in self.clients:
            if c.name and c is not exclude:
                c.send({"type": "system", "text": text})

    def _log(self, msg):
        if not self.log_path:
            return
        try:
            with open(self.log_path, "a", encoding="utf-8") as f:
                f.write(json.dumps(msg, ensure_ascii=False) + "\n")
        except OSError:
            pass

    def _recipients(self, sender, to):
        """Return the set of terminal names that should receive a message, or None if invalid."""
        if to == BROADCAST:
            return None, self._online_names() | {sender}
        if to.startswith(GROUP_PREFIX):
            gname = to[1:]
            members = self.groups.get(gname)
            if members is None:
                return f"no such group: {to}", None
            if sender not in members:
                return f"you are not a member of {to}", None
            return None, set(members)
        if to not in self.known:
            return f"unknown terminal: {to}", None
        return None, {to, sender}

    # ----- per-client loop -------------------------------------------------
    def _client_loop(self, client):
        reader = LineReader(client.sock)
        try:
            while True:
                try:
                    msg = reader.read()
                except (ValueError, UnicodeDecodeError):
                    client.send({"type": "error", "text": "malformed message"})
                    break
                except OSError:
                    break
                if msg is None:
                    break
                if not isinstance(msg, dict):
                    client.send({"type": "error", "text": "expected a JSON object"})
                    continue
                self._handle(client, msg)
        finally:
            self._disconnect(client)

    def _disconnect(self, client):
        with self.lock:
            if client in self.clients:
                self.clients.remove(client)
            name = client.name
        try:
            client.sock.close()
        except OSError:
            pass
        if name:
            with self.lock:
                if client.role == "terminal":
                    self._system(f"{name} disconnected")
                self._broadcast_presence()

    def _handle(self, client, msg):
        mtype = msg.get("type")
        if client.name is None:
            if mtype != "hello":
                client.send({"type": "error", "text": "say hello first"})
                return
            self._hello(client, msg)
            return
        handler = {
            "send": self._send,
            "group_create": self._group_create,
            "group_join": self._group_join,
            "group_leave": self._group_leave,
            "who": self._who,
            "history": self._history,
        }.get(mtype)
        if handler is None:
            client.send({"type": "error", "text": f"unknown message type: {mtype!r}"})
            return
        handler(client, msg)

    def _hello(self, client, msg):
        name = str(msg.get("name", ""))
        role = msg.get("role", "terminal")
        if role not in ("terminal", "board"):
            client.send({"type": "error", "text": "role must be 'terminal' or 'board'"})
            return
        if not valid_name(name):
            client.send({"type": "error", "text": "bad name: use 1-32 letters, digits, _ - ."})
            return
        with self.lock:
            if role == "terminal" and name in self._online_names():
                client.send({"type": "error", "text": f"name '{name}' is already online, pick another"})
                return
            client.name = name
            client.role = role
            self.clients.append(client)
            if role == "terminal":
                self.known[name] = time.time()
            welcome = self._presence_payload()
            welcome.update({"type": "welcome", "name": name, "history": self.history[-100:]})
            client.send(welcome)
            if role == "terminal":
                self._system(f"{name} connected", exclude=client)
            self._broadcast_presence()

    def _send(self, client, msg):
        if client.role != "terminal" and client.role != "board":
            return
        text = str(msg.get("text", "")).strip()
        to = str(msg.get("to", BROADCAST)).strip() or BROADCAST
        if not text:
            client.send({"type": "error", "text": "empty message"})
            return
        if len(text) > 8000:
            client.send({"type": "error", "text": "message too long (max 8000 chars)"})
            return
        with self.lock:
            err, recips = self._recipients(client.name, to)
            if err:
                client.send({"type": "error", "text": err})
                return
            message = {
                "type": "message",
                "id": self.next_id,
                "ts": time.time(),
                "from": client.name,
                "to": to,
                "text": text,
            }
            self.next_id += 1
            self.history.append(message)
            if len(self.history) > self.history_limit:
                del self.history[: len(self.history) - self.history_limit]
            self._log(message)
            for c in self.clients:
                if c.role == "board" or c.name in recips:
                    c.send(message)

    def _group_create(self, client, msg):
        gname = str(msg.get("name", ""))
        members = msg.get("members") or []
        if not valid_name(gname):
            client.send({"type": "error", "text": "bad group name"})
            return
        with self.lock:
            if gname in self.groups:
                client.send({"type": "error", "text": f"group #{gname} already exists"})
                return
            member_set = {client.name} if client.role == "terminal" else set()
            unknown = [m for m in members if m not in self.known]
            if unknown:
                client.send({"type": "error", "text": "unknown terminals: " + ", ".join(unknown)})
                return
            member_set |= set(members)
            self.groups[gname] = member_set
            self._system(f"{client.name} created #{gname} with " + ", ".join(sorted(member_set)))
            self._broadcast_presence()

    def _group_join(self, client, msg):
        gname = str(msg.get("name", ""))
        with self.lock:
            if gname not in self.groups:
                client.send({"type": "error", "text": f"no such group: #{gname}"})
                return
            self.groups[gname].add(client.name)
            self._system(f"{client.name} joined #{gname}")
            self._broadcast_presence()

    def _group_leave(self, client, msg):
        gname = str(msg.get("name", ""))
        with self.lock:
            if gname not in self.groups or client.name not in self.groups[gname]:
                client.send({"type": "error", "text": f"you are not in #{gname}"})
                return
            self.groups[gname].discard(client.name)
            if not self.groups[gname]:
                del self.groups[gname]
            self._system(f"{client.name} left #{gname}")
            self._broadcast_presence()

    def _who(self, client, msg):
        with self.lock:
            client.send(self._presence_payload())

    def _history(self, client, msg):
        limit = int(msg.get("limit", 50) or 50)
        with self.lock:
            for m in self.history[-limit:]:
                client.send(dict(m, replay=True))


def main():
    ap = argparse.ArgumentParser(description="Terminal Message Board bridge server")
    ap.add_argument("--host", default=DEFAULT_HOST)
    ap.add_argument("--port", type=int, default=DEFAULT_PORT)
    ap.add_argument("--log", default=None, help="append every message to this .jsonl file")
    args = ap.parse_args()
    bridge = Bridge(args.host, args.port, args.log).start()
    print(f"[bridge] listening on {bridge.host}:{bridge.port}  (Ctrl+C to stop)")
    bridge.serve_forever()


if __name__ == "__main__":
    main()
