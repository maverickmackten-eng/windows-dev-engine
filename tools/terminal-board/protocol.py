"""Shared wire protocol for the Terminal Message Board.

Transport: plain TCP on localhost, newline-delimited JSON (one object per line).

Client -> Bridge
    {"type": "hello",        "name": "<terminal name>", "role": "terminal" | "board"}
    {"type": "send",         "to": "*" | "<name>" | "#<group>", "text": "..."}
    {"type": "group_create", "name": "<group>", "members": ["<name>", ...]}
    {"type": "group_join",   "name": "<group>"}
    {"type": "group_leave",  "name": "<group>"}
    {"type": "who"}
    {"type": "history",      "limit": 50}

Bridge -> Client
    {"type": "welcome",  "name": ..., "contacts": [...], "groups": {...}, "history": [...]}
    {"type": "message",  "id": int, "ts": float, "from": ..., "to": ..., "text": ...}
    {"type": "presence", "contacts": [{"name": ..., "online": bool}], "groups": {name: [members]}}
    {"type": "system",   "text": "..."}
    {"type": "error",    "text": "..."}
"""
import json
import re
import socket

DEFAULT_HOST = "127.0.0.1"
DEFAULT_PORT = 7788
BROADCAST = "*"
GROUP_PREFIX = "#"
MAX_LINE = 64 * 1024

_NAME_RE = re.compile(r"^[A-Za-z0-9_\-.]{1,32}$")


def valid_name(name: str) -> bool:
    """Terminal and group names: letters, digits, _ - . up to 32 chars."""
    return bool(name) and bool(_NAME_RE.match(name))


def encode(obj: dict) -> bytes:
    return (json.dumps(obj, ensure_ascii=False) + "\n").encode("utf-8")


class LineReader:
    """Reads newline-delimited JSON objects from a socket. Returns None on EOF."""

    def __init__(self, sock: socket.socket):
        self.sock = sock
        self.buf = b""

    def read(self):
        while b"\n" not in self.buf:
            if len(self.buf) > MAX_LINE:
                raise ValueError("line too long")
            chunk = self.sock.recv(4096)
            if not chunk:
                return None
            self.buf += chunk
        line, _, self.buf = self.buf.partition(b"\n")
        line = line.strip()
        if not line:
            return self.read()
        return json.loads(line.decode("utf-8"))
