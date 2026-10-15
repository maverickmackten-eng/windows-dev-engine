"""One-command launcher.

    python launcher.py                 start the bridge (if not running) and the board window
    python launcher.py new Maverick    open a NEW terminal window named Maverick
    python launcher.py new             open a new terminal window and ask for a name
    python launcher.py bridge          start only the bridge in this console

On Windows each `new` opens a fresh console window (start cmd /k ...).
On macOS/Linux it falls back to printing the command to run.
"""
import os
import socket
import subprocess
import time
import sys

from protocol import DEFAULT_HOST, DEFAULT_PORT

HERE = os.path.dirname(os.path.abspath(__file__))
PY = sys.executable


def bridge_running(host=DEFAULT_HOST, port=DEFAULT_PORT):
    try:
        with socket.create_connection((host, port), timeout=0.5):
            return True
    except OSError:
        return False


def spawn_console(args, title):
    """Open a new console window running `python <args>`."""
    if sys.platform == "win32":
        cmd = ["cmd", "/c", "start", f'"{title}"', "cmd", "/k", PY] + args
        subprocess.Popen(" ".join(_q(a) for a in cmd), shell=True, cwd=HERE)
        return True
    # best-effort on other platforms
    for term in (["x-terminal-emulator", "-e"], ["gnome-terminal", "--"], ["xterm", "-e"]):
        try:
            subprocess.Popen(term + [PY] + args, cwd=HERE)
            return True
        except FileNotFoundError:
            continue
    print("open a terminal and run:  " + " ".join([PY] + args))
    return False


def _q(a):
    return f'"{a}"' if " " in a and not a.startswith('"') else a


def start_bridge_background(port):
    if bridge_running(port=port):
        print(f"[launcher] bridge already running on {DEFAULT_HOST}:{port}")
        return None
    flags = 0
    if sys.platform == "win32":
        flags = subprocess.CREATE_NEW_CONSOLE
    proc = subprocess.Popen([PY, os.path.join(HERE, "bridge.py"), "--port", str(port),
                             "--log", os.path.join(HERE, "messages.jsonl")],
                            cwd=HERE, creationflags=flags)
    for _ in range(40):
        if bridge_running(port=port):
            break
        time.sleep(0.1)
    print(f"[launcher] bridge started (pid {proc.pid}) on {DEFAULT_HOST}:{port}")
    return proc


def main(argv):
    port = DEFAULT_PORT
    if "--port" in argv:
        i = argv.index("--port")
        port = int(argv[i + 1])
        del argv[i:i + 2]
    cmd = argv[0] if argv else "board"

    if cmd == "bridge":
        os.execv(PY, [PY, os.path.join(HERE, "bridge.py"), "--port", str(port)])
    if cmd == "new":
        if not bridge_running(port=port):
            start_bridge_background(port)
        name = argv[1] if len(argv) > 1 else None
        args = [os.path.join(HERE, "terminal.py"), "--port", str(port)]
        if name:
            args += ["--name", name]
        spawn_console(args, f"Terminal - {name or 'new'}")
        return 0
    if cmd in ("board", "start"):
        start_bridge_background(port)
        subprocess.call([PY, os.path.join(HERE, "board.py"), "--port", str(port)], cwd=HERE)
        return 0
    print(__doc__)
    return 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
