"""Message Board window: a live chat view of everything flowing through the bridge.

Run:  python board.py [--port 7788]

Left pane lists Everyone, every #group and every terminal that has ever connected.
Click one to filter the conversation. The "View as" box picks which terminal is
"you" (its bubbles go on the right in accent blue). The input box at the bottom
lets the board itself post to the selected conversation as the "Board" terminal.

Stdlib only (tkinter ships with the Windows Python installer; tick "tcl/tk" if
you unticked it).
"""
import argparse
import queue
import socket
import sys
import threading
import time
import tkinter as tk
from tkinter import ttk

from protocol import (BROADCAST, DEFAULT_HOST, DEFAULT_PORT, GROUP_PREFIX,
                      LineReader, encode)

# Palette from agent/02_DESIGN_STANDARDS.md (Dark Military)
BG_DEEP = "#0A0D12"
BG_PANEL = "#12171F"
BG_ELEVATED = "#1C2333"
BORDER = "#2A3347"
BORDER_ACTIVE = "#3D5A8A"
ACCENT = "#00B4FF"
ACCENT_ORANGE = "#FF6B35"
SUCCESS = "#00FF94"
DANGER = "#FF2D55"
GOLD = "#FFD700"
TEXT = "#FFFFFF"
TEXT_SECONDARY = "#8899AA"
TEXT_DISABLED = "#445566"

BUBBLE_OTHER = BG_ELEVATED
BUBBLE_ME = "#0B5F86"
BUBBLE_SYSTEM = BG_PANEL
SENDER_COLORS = [SUCCESS, GOLD, ACCENT_ORANGE, "#C77DFF", "#5BE7C4", "#FF8FAB", "#7FB3FF"]

FONT = ("Segoe UI", 10)
FONT_BOLD = ("Segoe UI", 10, "bold")
FONT_SMALL = ("Segoe UI", 8)
FONT_TITLE = ("Segoe UI", 14, "bold")
FONT_MONO = ("Consolas", 10)

EVERYONE = "Everyone"
BOARD_NAME = "Board"


class BridgeLink:
    """Socket connection to the bridge; incoming objects land in a queue."""

    def __init__(self, host, port, inbox):
        self.host, self.port, self.inbox = host, port, inbox
        self.sock = None
        self.alive = False

    def connect(self):
        self.sock = socket.create_connection((self.host, self.port), timeout=5)
        self.sock.settimeout(None)
        self.sock.sendall(encode({"type": "hello", "name": BOARD_NAME, "role": "board"}))
        self.alive = True
        threading.Thread(target=self._reader, daemon=True).start()

    def _reader(self):
        reader = LineReader(self.sock)
        try:
            while self.alive:
                msg = reader.read()
                if msg is None:
                    break
                self.inbox.put(msg)
        except (OSError, ValueError):
            pass
        self.alive = False
        self.inbox.put({"type": "_disconnected"})

    def send(self, obj):
        if not self.alive:
            return
        try:
            self.sock.sendall(encode(obj))
        except OSError:
            self.alive = False

    def close(self):
        self.alive = False
        try:
            self.sock.close()
        except (OSError, AttributeError):
            pass


class BubbleCanvas(tk.Canvas):
    """Scrollable canvas that draws chat bubbles."""

    PAD = 14
    GAP = 10
    RADIUS = 14
    MAX_WIDTH_FRACTION = 0.68

    def __init__(self, master, **kw):
        super().__init__(master, bg=BG_DEEP, highlightthickness=0, **kw)
        self.items = []  # list of (kind, payload)
        self.y = self.PAD
        self.bind("<Configure>", lambda e: self.redraw())
        self.bind_all("<MouseWheel>", self._wheel)
        self.bind_all("<Button-4>", lambda e: self.yview_scroll(-3, "units"))
        self.bind_all("<Button-5>", lambda e: self.yview_scroll(3, "units"))

    def _wheel(self, event):
        self.yview_scroll(int(-event.delta / 40), "units")

    def clear(self):
        self.items = []
        self.redraw()

    def add_message(self, sender, text, ts, mine, color, context=None):
        self.items.append(("msg", dict(sender=sender, text=text, ts=ts, mine=mine,
                                       color=color, context=context)))
        self._draw_item(self.items[-1])
        self._finish()

    def add_system(self, text, ts):
        self.items.append(("sys", dict(text=text, ts=ts)))
        self._draw_item(self.items[-1])
        self._finish()

    def redraw(self):
        self.delete("all")
        self.y = self.PAD
        for item in self.items:
            self._draw_item(item)
        self._finish()

    def _finish(self):
        self.configure(scrollregion=(0, 0, self.winfo_width(), self.y + self.PAD))
        self.yview_moveto(1.0)

    def _rounded(self, x1, y1, x2, y2, r, **kw):
        pts = [x1 + r, y1, x2 - r, y1, x2, y1, x2, y1 + r, x2, y2 - r, x2, y2,
               x2 - r, y2, x1 + r, y2, x1, y2, x1, y2 - r, x1, y1 + r, x1, y1]
        return self.create_polygon(pts, smooth=True, **kw)

    def _draw_item(self, item):
        kind, p = item
        width = max(self.winfo_width(), 300)
        if kind == "sys":
            t = time.strftime("%H:%M", time.localtime(p["ts"]))
            tid = self.create_text(width / 2, self.y, text=f"{t}  {p['text']}", fill=TEXT_SECONDARY,
                                   font=FONT_SMALL, anchor="n")
            x1, y1, x2, y2 = self.bbox(tid)
            self.y = y2 + self.GAP
            return

        max_w = int(width * self.MAX_WIDTH_FRACTION)
        t = time.strftime("%H:%M:%S", time.localtime(p["ts"]))
        header = p["sender"] if p["context"] is None else f"{p['sender']}  ·  {p['context']}"
        # draw text first (hidden offscreen position), measure, then place bubble behind
        hid = self.create_text(0, 0, text=header, fill=p["color"], font=FONT_BOLD, anchor="nw",
                               width=max_w)
        bid = self.create_text(0, 0, text=p["text"], fill=TEXT, font=FONT, anchor="nw", width=max_w)
        tid = self.create_text(0, 0, text=t, fill=TEXT_SECONDARY, font=FONT_SMALL, anchor="nw")
        hw, hh = self._size(hid)
        bw, bh = self._size(bid)
        tw, th = self._size(tid)
        inner_w = max(hw, bw, tw + 40)
        inner_h = hh + 2 + bh + 2 + th
        box_w = inner_w + 2 * self.PAD
        box_h = inner_h + 2 * self.PAD - 4
        if p["mine"]:
            x1 = width - self.PAD - box_w
        else:
            x1 = self.PAD
        y1 = self.y
        x2, y2 = x1 + box_w, y1 + box_h
        fill = BUBBLE_ME if p["mine"] else BUBBLE_OTHER
        self._rounded(x1, y1, x2, y2, self.RADIUS, fill=fill, outline="")
        # tail accent line
        self.create_line(x2 - 1 if p["mine"] else x1 + 1, y1 + self.RADIUS,
                         x2 - 1 if p["mine"] else x1 + 1, y2 - self.RADIUS,
                         fill=p["color"], width=2)
        cx = x1 + self.PAD
        cy = y1 + self.PAD - 2
        self.coords(hid, cx, cy)
        self.coords(bid, cx, cy + hh + 2)
        self.coords(tid, x2 - self.PAD - tw, cy + hh + 2 + bh + 2)
        for i in (hid, bid, tid):
            self.tag_raise(i)
        self.y = y2 + self.GAP

    def _size(self, item_id):
        x1, y1, x2, y2 = self.bbox(item_id)
        return x2 - x1, y2 - y1


class BoardApp:
    def __init__(self, root, host, port):
        self.root = root
        self.host, self.port = host, port
        self.inbox = queue.Queue()
        self.link = BridgeLink(host, port, self.inbox)
        self.messages = []       # every message dict seen
        self.events = []         # (ts, text) system lines
        self.contacts = []       # [{"name", "online"}]
        self.groups = {}         # name -> [members]
        self.selected = EVERYONE
        self.view_as = tk.StringVar(value="")
        self.sender_colors = {}
        self._build_ui()
        self._connect()
        self.root.after(80, self._poll)

    # ----- UI ----------------------------------------------------------------
    def _build_ui(self):
        r = self.root
        r.title("Terminal Message Board")
        r.configure(bg=BG_DEEP)
        r.geometry("1000x680")
        r.minsize(720, 460)

        style = ttk.Style()
        try:
            style.theme_use("clam")
        except tk.TclError:
            pass
        style.configure("TCombobox", fieldbackground=BG_PANEL, background=BG_ELEVATED,
                        foreground=TEXT, arrowcolor=TEXT, bordercolor=BORDER)
        style.map("TCombobox", fieldbackground=[("readonly", BG_PANEL)],
                  foreground=[("readonly", TEXT)])
        r.option_add("*TCombobox*Listbox.background", BG_PANEL)
        r.option_add("*TCombobox*Listbox.foreground", TEXT)
        r.option_add("*TCombobox*Listbox.selectBackground", BORDER_ACTIVE)

        # title bar
        top = tk.Frame(r, bg=BG_PANEL, height=48)
        top.pack(fill="x")
        top.pack_propagate(False)
        tk.Label(top, text="◆ TERMINAL MESSAGE BOARD", bg=BG_PANEL, fg=ACCENT, font=FONT_TITLE).pack(side="left", padx=16)
        self.status = tk.Label(top, text="● connecting…", bg=BG_PANEL, fg=GOLD, font=FONT)
        self.status.pack(side="right", padx=16)
        tk.Label(top, text="View as:", bg=BG_PANEL, fg=TEXT_SECONDARY, font=FONT).pack(side="right")
        self.view_combo = ttk.Combobox(top, textvariable=self.view_as, state="readonly", width=16)
        self.view_combo.pack(side="right", padx=(0, 10))
        self.view_combo.bind("<<ComboboxSelected>>", lambda e: self._render())

        body = tk.Frame(r, bg=BG_DEEP)
        body.pack(fill="both", expand=True)

        # contacts pane
        left = tk.Frame(body, bg=BG_PANEL, width=220)
        left.pack(side="left", fill="y")
        left.pack_propagate(False)
        tk.Label(left, text="CONVERSATIONS", bg=BG_PANEL, fg=TEXT_SECONDARY, font=FONT_SMALL, anchor="w").pack(fill="x", padx=12, pady=(12, 4))
        self.contact_list = tk.Listbox(left, bg=BG_PANEL, fg=TEXT, selectbackground=BORDER_ACTIVE,
                                       selectforeground=TEXT, activestyle="none", bd=0,
                                       highlightthickness=0, font=FONT_MONO)
        self.contact_list.pack(fill="both", expand=True, padx=8, pady=4)
        self.contact_list.bind("<<ListboxSelect>>", self._on_select)
        self.hint = tk.Label(left, text="open a new terminal:\npython terminal.py --name NAME",
                             bg=BG_PANEL, fg=TEXT_DISABLED, font=FONT_SMALL, justify="left")
        self.hint.pack(fill="x", padx=12, pady=10)

        # chat pane
        right = tk.Frame(body, bg=BG_DEEP)
        right.pack(side="left", fill="both", expand=True)
        self.header = tk.Label(right, text=EVERYONE, bg=BG_DEEP, fg=TEXT, font=FONT_BOLD, anchor="w")
        self.header.pack(fill="x", padx=16, pady=(12, 0))
        self.subheader = tk.Label(right, text="", bg=BG_DEEP, fg=TEXT_SECONDARY, font=FONT_SMALL, anchor="w")
        self.subheader.pack(fill="x", padx=16)

        canvas_frame = tk.Frame(right, bg=BG_DEEP)
        canvas_frame.pack(fill="both", expand=True, padx=8, pady=8)
        self.canvas = BubbleCanvas(canvas_frame)
        sb = tk.Scrollbar(canvas_frame, command=self.canvas.yview, bg=BG_PANEL, troughcolor=BG_DEEP, bd=0)
        self.canvas.configure(yscrollcommand=sb.set)
        sb.pack(side="right", fill="y")
        self.canvas.pack(side="left", fill="both", expand=True)

        # input row
        bottom = tk.Frame(right, bg=BG_PANEL)
        bottom.pack(fill="x")
        self.entry = tk.Entry(bottom, bg=BG_ELEVATED, fg=TEXT, insertbackground=ACCENT, bd=0,
                              font=FONT, highlightthickness=1, highlightbackground=BORDER,
                              highlightcolor=ACCENT)
        self.entry.pack(side="left", fill="x", expand=True, padx=12, pady=12, ipady=6)
        self.entry.bind("<Return>", lambda e: self._send_from_board())
        self.send_btn = tk.Button(bottom, text="SEND ▶", bg=ACCENT, fg=BG_DEEP, bd=0, font=FONT_BOLD,
                                  activebackground=BORDER_ACTIVE, activeforeground=TEXT,
                                  command=self._send_from_board, padx=14, pady=6, cursor="hand2")
        self.send_btn.pack(side="right", padx=(0, 12))

    # ----- networking --------------------------------------------------------
    def _connect(self):
        try:
            self.link.connect()
            self.status.configure(text=f"● bridge {self.host}:{self.port}", fg=SUCCESS)
        except OSError as e:
            self.status.configure(text=f"✕ bridge offline ({e.strerror or e})", fg=DANGER)
            self.root.after(2000, self._connect)

    def _poll(self):
        try:
            while True:
                msg = self.inbox.get_nowait()
                self._dispatch(msg)
        except queue.Empty:
            pass
        self.root.after(80, self._poll)

    def _dispatch(self, msg):
        t = msg.get("type")
        if t == "welcome":
            self.messages = list(msg.get("history", []))
            self._apply_presence(msg)
            self._render()
        elif t == "message":
            self.messages.append(msg)
            if self._visible(msg):
                self._draw_message(msg)
        elif t == "presence":
            self._apply_presence(msg)
        elif t == "system":
            self.events.append((time.time(), msg["text"]))
            self.canvas.add_system(msg["text"], time.time())
        elif t == "error":
            self.canvas.add_system("error: " + msg["text"], time.time())
        elif t == "_disconnected":
            self.status.configure(text="✕ bridge disconnected, retrying…", fg=DANGER)
            self.link = BridgeLink(self.host, self.port, self.inbox)
            self.root.after(1500, self._connect)

    # ----- state -------------------------------------------------------------
    def _apply_presence(self, msg):
        self.contacts = msg.get("contacts", [])
        self.groups = msg.get("groups", {})
        names = [c["name"] for c in self.contacts]
        self.view_combo.configure(values=names)
        if (not self.view_as.get() or self.view_as.get() not in names) and names:
            self.view_as.set(names[0])
            self._render()
        self._fill_contact_list()

    def _fill_contact_list(self):
        self.contact_list.delete(0, "end")
        self._rows = [EVERYONE]
        self.contact_list.insert("end", "◆ Everyone")
        for g in sorted(self.groups):
            self._rows.append(GROUP_PREFIX + g)
            self.contact_list.insert("end", f"# {g}")
        for c in self.contacts:
            self._rows.append(c["name"])
            dot = "●" if c["online"] else "○"
            self.contact_list.insert("end", f"{dot} {c['name']}")
            self.contact_list.itemconfig("end", fg=SUCCESS if c["online"] else TEXT_DISABLED)
        try:
            idx = self._rows.index(self.selected)
        except ValueError:
            idx = 0
            self.selected = EVERYONE
        self.contact_list.selection_clear(0, "end")
        self.contact_list.selection_set(idx)
        self._update_header()

    def _on_select(self, _event):
        sel = self.contact_list.curselection()
        if not sel:
            return
        self.selected = self._rows[sel[0]]
        self._update_header()
        self._render()
        self.entry.focus_set()

    def _update_header(self):
        s = self.selected
        if s == EVERYONE:
            self.header.configure(text="◆ Everyone")
            online = [c["name"] for c in self.contacts if c["online"]]
            self.subheader.configure(text="group chat  ·  online: " + (", ".join(online) or "nobody"))
        elif s.startswith(GROUP_PREFIX):
            self.header.configure(text=s)
            self.subheader.configure(text="members: " + ", ".join(self.groups.get(s[1:], [])))
        else:
            self.header.configure(text="● " + s)
            self.subheader.configure(text=f"direct messages with {s}")

    def _color_for(self, name):
        if name == self.view_as.get():
            return ACCENT
        if name not in self.sender_colors:
            self.sender_colors[name] = SENDER_COLORS[len(self.sender_colors) % len(SENDER_COLORS)]
        return self.sender_colors[name]

    def _visible(self, m):
        s = self.selected
        if s == EVERYONE:
            return m["to"] == BROADCAST
        if s.startswith(GROUP_PREFIX):
            return m["to"] == s
        # direct messages involving the selected terminal
        return m["to"] not in (BROADCAST,) and not m["to"].startswith(GROUP_PREFIX) \
            and (m["from"] == s or m["to"] == s)

    def _render(self):
        self.canvas.clear()
        for m in self.messages:
            if self._visible(m):
                self._draw_message(m)

    def _draw_message(self, m):
        mine = m["from"] == self.view_as.get()
        context = None
        if self.selected != EVERYONE and not self.selected.startswith(GROUP_PREFIX):
            context = f"to {m['to']}"
        self.canvas.add_message(m["from"], m["text"], m["ts"], mine, self._color_for(m["from"]), context)

    def _send_from_board(self):
        text = self.entry.get().strip()
        if not text:
            return
        to = BROADCAST if self.selected == EVERYONE else self.selected
        self.link.send({"type": "send", "to": to, "text": text})
        self.entry.delete(0, "end")

    def close(self):
        self.link.close()
        self.root.destroy()


def main():
    ap = argparse.ArgumentParser(description="Terminal Message Board window")
    ap.add_argument("--host", default=DEFAULT_HOST)
    ap.add_argument("--port", type=int, default=DEFAULT_PORT)
    args = ap.parse_args()
    root = tk.Tk()
    app = BoardApp(root, args.host, args.port)
    root.protocol("WM_DELETE_WINDOW", app.close)
    root.mainloop()


if __name__ == "__main__":
    sys.exit(main())
