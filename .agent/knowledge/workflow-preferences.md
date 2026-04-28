# Workflow Preferences

## User Preferences
- Prefer autonomous continuation until verified: YES — run full phase without stopping unless blocked
- Prefer log files over terminal-only output: YES — use `run_with_logs.py` for all subprocess calls
- Prefer research after unclear failures: YES — especially for Win32 interop and robocopy exit codes
- Prefer path-safe code and no brittle hard-coded paths: YES — use `Environment.GetFolderPath` and `Path.Combine`, never string concat
- Prefer strong UI/UX research before building local launchers: YES — spec §4 is the UI contract; follow it exactly
- Preferred repo and control-file structure: `.agent/` workspace at repo root; `src/` for C# source; `tests/` for xUnit; `docs/` for spec and architecture diagrams
