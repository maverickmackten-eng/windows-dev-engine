"""Named terminal client. Open as many as you like, each with its own name.

Run:  python terminal.py --name Maverick [--port 7788]

Type a line and press Enter to send to the current target (default: everyone).
Commands:
    /to <name> <text>     direct message one terminal
    /all <text>           message everyone
    /g <group> <text>     message a group (with or without leading #)
    /target <name|#group|*>   change the default target for plain lines
    /group create <name> [member ...]   create a group (you are added automatically)
    /group join <name>    join an existing group
    /group leave <name>   leave a group
    /who                  list terminals and groups
    /history [n]          replay the last n messages (default 50)
    /quit                 exit
"""
import argparse
import socket
import sys
import threading
import time

from protocol import (BROADCAST, DEFAULT_HOST, DEFAULT_PORT, GROUP_PREFIX,
                      LineReader, encode, valid_name)

ANSI = sys.stdout.isatty()


def color(code, text):
    return f"\033[{code}m{text}\033[0m" if ANSI else text


def enable_windows_ansi():
    if sys.platform != "win32":
        return
    try:
        import ctypes
        kernel32 = ctypes.windll.kernel32
        handle = kernel32.GetStdHandle(-11)
        mode = ctypes.c_uint32()
        if kernel32.GetConsoleMode(handle, ctypes.byref(mode)):
            kernel32.SetConsoleMode(handle, mode.value | 0x0004)
    except Exception:
        pass


def fmt_message(me, m):
    t = time.strftime("%H:%M:%S", time.localtime(m["ts"]))
    sender = m["from"]
    to = m["to"]
    if to == BROADCAST:
        tag = color("36", "[all]")
    elif to.startswith(GROUP_PREFIX):
        tag = color("35", f"[{to}]")
    else:
        other = to if sender == me else sender
        tag = color("33", f"[dm:{other}]")
    who = color("1;32", "you") if sender == me else color("1;34", sender)
    return f"{color('90', t)} {tag} {who}: {m['text']}"


class Terminal:
    def __init__(self, name, host, port):
        self.name = name
        self.host = host
        self.port = port
        self.sock = None
        self.target = BROADCAST
        self.alive = True

    def connect(self):
        self.sock = socket.create_connection((self.host, self.port), timeout=5)
        self.sock.settimeout(None)
        self.sock.sendall(encode({"type": "hello", "name": self.name, "role": "terminal"}))
        reader = LineReader(self.sock)
        first = reader.read()
        if first is None:
            raise ConnectionError("bridge closed the connection")
        if first.get("type") == "error":
            raise ConnectionError(first.get("text"))
        self._welcome(first)
        threading.Thread(target=self._reader, args=(reader,), daemon=True).start()

    def _welcome(self, w):
        online = [c["name"] for c in w.get("contacts", []) if c["online"] and c["name"] != self.name]
        print(color("1;32", f"connected to bridge as '{self.name}'"))
        print(color("90", "online: " + (", ".join(online) if online else "nobody else yet")))
        if w.get("groups"):
            print(color("90", "groups: " + ", ".join("#" + g for g in w["groups"])))
        hist = w.get("history", [])
        if hist:
            print(color("90", f"--- last {len(hist)} messages ---"))
            for m in hist:
                print(fmt_message(self.name, m))
            print(color("90", "--- end of history ---"))
        print(color("90", "type a message and press Enter. /help for commands."))

    def _reader(self, reader):
        try:
            while self.alive:
                msg = reader.read()
                if msg is None:
                    break
                self._on_message(msg)
        except (OSError, ValueError):
            pass
        if self.alive:
            print("\n" + color("1;31", "lost connection to bridge"))
            self.alive = False

    def _on_message(self, msg):
        t = msg.get("type")
        if t == "message":
            if msg["from"] == self.name and not msg.get("replay"):
                return  # already echoed locally
            print("\r" + fmt_message(self.name, msg))
        elif t == "system":
            print("\r" + color("90", "* " + msg["text"]))
        elif t == "error":
            print("\r" + color("1;31", "! " + msg["text"]))
        elif t == "presence":
            pass  # silent; /who prints it on demand
        sys.stdout.flush()

    def send(self, obj):
        try:
            self.sock.sendall(encode(obj))
        except OSError:
            print(color("1;31", "send failed: not connected"))
            self.alive = False

    # ----- command handling ------------------------------------------------
    def handle_line(self, line):
        line = line.rstrip("\n")
        if not line.strip():
            return True
        if not line.startswith("/"):
            self._send_text(self.target, line)
            return True
        parts = line.split(" ", 1)
        cmd = parts[0].lower()
        rest = parts[1] if len(parts) > 1 else ""
        if cmd in ("/quit", "/exit", "/q"):
            return False
        if cmd == "/help":
            print(__doc__)
        elif cmd == "/all":
            self._send_text(BROADCAST, rest)
        elif cmd in ("/to", "/dm"):
            name, _, text = rest.partition(" ")
            if not name or not text.strip():
                print(color("1;31", "usage: /to <name> <text>"))
            else:
                self._send_text(name, text)
        elif cmd == "/g":
            gname, _, text = rest.partition(" ")
            if not gname or not text.strip():
                print(color("1;31", "usage: /g <group> <text>"))
            else:
                self._send_text(self._gref(gname), text)
        elif cmd == "/target":
            tgt = rest.strip()
            if not tgt:
                print(color("90", f"current target: {self.target}"))
            else:
                self.target = BROADCAST if tgt == "all" else tgt
                print(color("90", f"default target is now {self.target}"))
        elif cmd == "/group":
            self._group_cmd(rest)
        elif cmd == "/who":
            self.send({"type": "who"})
            self._print_presence_soon()
        elif cmd == "/history":
            try:
                n = int(rest.strip() or 50)
            except ValueError:
                n = 50
            self.send({"type": "history", "limit": n})
        else:
            print(color("1;31", f"unknown command {cmd}. /help for the list"))
        return True

    @staticmethod
    def _gref(gname):
        gname = gname.strip()
        return gname if gname.startswith(GROUP_PREFIX) else GROUP_PREFIX + gname

    def _send_text(self, to, text):
        text = text.strip()
        if not text:
            return
        if to != BROADCAST and not to.startswith(GROUP_PREFIX) and not valid_name(to):
            print(color("1;31", f"bad target name: {to}"))
            return
        self.send({"type": "send", "to": to, "text": text})
        print(fmt_message(self.name, {"ts": time.time(), "from": self.name, "to": to, "text": text}))

    def _group_cmd(self, rest):
        words = rest.split()
        if not words:
            print(color("1;31", "usage: /group create|join|leave <name> [members...]"))
            return
        sub = words[0].lower()
        if len(words) < 2:
            print(color("1;31", "group name required"))
            return
        gname = words[1].lstrip(GROUP_PREFIX)
        if sub == "create":
            self.send({"type": "group_create", "name": gname, "members": words[2:]})
        elif sub == "join":
            self.send({"type": "group_join", "name": gname})
        elif sub == "leave":
            self.send({"type": "group_leave", "name": gname})
        else:
            print(color("1;31", f"unknown group command: {sub}"))

    def _print_presence_soon(self):
        # Presence arrives asynchronously; the reader prints nothing for it, so
        # temporarily hook it.
        original = self._on_message

        def hooked(msg):
            if msg.get("type") == "presence":
                self._on_message = original
                online = [c["name"] for c in msg["contacts"] if c["online"]]
                offline = [c["name"] for c in msg["contacts"] if not c["online"]]
                print("\r" + color("90", "online : ") + ", ".join(online))
                if offline:
                    print(color("90", "offline: ") + ", ".join(offline))
                for g, members in msg.get("groups", {}).items():
                    print(color("90", f"#{g}: ") + ", ".join(members))
                sys.stdout.flush()
            else:
                original(msg)

        self._on_message = hooked

    def run(self):
        try:
            while self.alive:
                try:
                    line = input()
                except EOFError:
                    break
                if not self.handle_line(line):
                    break
        except KeyboardInterrupt:
            pass
        finally:
            self.alive = False
            try:
                self.sock.close()
            except OSError:
                pass
            print(color("90", "bye"))


def main():
    enable_windows_ansi()
    ap = argparse.ArgumentParser(description="Terminal Message Board client")
    ap.add_argument("--name", help="your terminal name (asked interactively if omitted)")
    ap.add_argument("--host", default=DEFAULT_HOST)
    ap.add_argument("--port", type=int, default=DEFAULT_PORT)
    args = ap.parse_args()

    name = args.name
    while not valid_name(name or ""):
        if name:
            print("names are 1-32 chars: letters, digits, _ - .")
        try:
            name = input("terminal name: ").strip()
        except EOFError:
            return 1

    term = Terminal(name, args.host, args.port)
    try:
        term.connect()
    except (OSError, ConnectionError) as e:
        print(color("1;31", f"could not connect to bridge at {args.host}:{args.port}: {e}"))
        print("start it first:  python launcher.py   (or python bridge.py)")
        return 1
    term.run()
    return 0


if __name__ == "__main__":
    sys.exit(main())
