# Terminal Message Board

A local message board for Windows (also runs on macOS/Linux). One window shows
chat bubbles; any number of named terminals on the same machine post to it and
to each other. Think "Grok bot message board", except each bot is a terminal
you open and name yourself.

**Python 3.9+ only, no pip dependencies.** Uses the standard library
(`socket`, `threading`, `tkinter`). On Windows make sure the Python installer's
"tcl/tk and IDLE" option was ticked (it is by default).

## Pieces

| File | What it is |
|---|---|
| `bridge.py` | The hub. A tiny TCP server on `127.0.0.1:7788` that every terminal and the board connect to. Routes messages, keeps history, tracks groups. |
| `board.py` | The window. Dark-military themed chat view with bubbles, a contacts/groups pane, a "View as" selector, and its own input box. |
| `terminal.py` | A named terminal client. Open one per person. |
| `launcher.py` | Convenience: starts bridge + board, or spawns a new named terminal window. |
| `start_board.bat` / `new_terminal.bat` | Double-click helpers for Windows. |
| `tests/` | Headless tests for the bridge (`python -m unittest discover tests`). |

## Quick start (Windows)

```bat
cd tools\terminal-board
start_board.bat              :: starts the bridge and opens the board window
new_terminal.bat Maverick    :: opens a new console named Maverick
new_terminal.bat Friend      :: opens another one for your friend
```

Or without the .bat files:

```bat
python launcher.py            :: bridge + board
python launcher.py new Maverick
python launcher.py new Friend
```

Type in either console and press Enter. The message shows up in the other
console and as a bubble on the board.

## Terminal commands

| Command | Does |
|---|---|
| `hello there` | plain text goes to the current target (default: everyone) |
| `/to Friend hi` | direct message one terminal |
| `/all hi` | message everyone |
| `/g dev hi` | message group `#dev` |
| `/target Friend` | make plain lines go to Friend (`/target *` for everyone, `/target #dev` for a group) |
| `/group create dev Friend Bob` | create group `#dev` with those members (you are added automatically) |
| `/group join dev` / `/group leave dev` | join or leave a group |
| `/who` | list online/offline terminals and groups |
| `/history 20` | replay the last 20 messages |
| `/help` / `/quit` | the usual |

Names: 1 to 32 characters, letters/digits/`_`/`-`/`.`. Two terminals cannot be
online with the same name at once.

## The board window

* **Left pane:** `Everyone`, each `#group`, and every terminal ever seen
  (green dot = online, grey = offline). Click to filter the conversation.
* **View as:** pick which terminal counts as "you". Its bubbles sit on the
  right in accent blue; everyone else gets a stable colour on the left.
* **Input box:** posts to the selected conversation as the terminal `Board`.
* The board reconnects automatically if the bridge restarts.

## Message log

The launcher starts the bridge with `--log messages.jsonl`, so every message is
appended to `tools/terminal-board/messages.jsonl` (git-ignored). Run
`python bridge.py` without `--log` if you do not want a file.

## Security scope

The bridge binds to `127.0.0.1` only: nothing leaves your machine, and nothing
on your network can connect to it. There is no authentication because every
client is a process on the same computer. If you ever want a friend on
*another* machine to join, do not simply bind `0.0.0.0`; that needs a proper
transport (TLS or an SSH tunnel) and is out of scope for this tool.

## Run the tests

```bat
cd tools\terminal-board
python -m unittest discover tests -v
```
