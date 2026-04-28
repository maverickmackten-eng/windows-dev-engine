# Understanding Proof

## Files Read
- File: `Mavericks-RoboCopy.ps1` — Why it matters: Main app; all UI controls, arg builder, output parser, pause/resume
- File: `Mav-AppTemplate.psm1` — Why it matters: Framework; all exported functions, data structures ($app $src $dst etc.), theming
- File: `Console-Run.ps1` — Why it matters: CLI variant, no WinForms, same arg-builder logic
- File: `Watch-Transfers.ps1` — Why it matters: Auto-booster background service, FileSystemWatcher, Win32 EnumWindows
- File: `Reset-State.ps1` — Why it matters: Test cleanup tool, wipes settings.json and state
- File: `Mavericks-RoboCopy.Tests.ps1` — Why it matters: 14 Pester assertions define contract for arg builder and completion logic

## End-to-End Understanding
- What the system does: GUI wrapper around robocopy.exe with pre-scan, live stats, pause/resume, auto-boost, file-type breakdown, tabbed log, and post-completion actions
- Main components: UI layer (WinForms via psm1 framework), RoboCopy wrapper (arg builder + process spawn), output parser (regex pipeline), stats engine, pause/resume (NtSuspendProcess P/Invoke), auto-booster (FileSystemWatcher service), settings manager (JSON), log router (3 tabs)
- How they connect: ps1 main calls psm1 exported functions to build UI → user triggers copy → arg builder constructs robocopy CLI → process spawned in background runspace → ConcurrentQueue feeds UI timer → output parser fires regex → stats/log updated → completion block fires post-actions
- Startup or execution flow: Load settings.json → build UI → pre-scan on Start → spawn robocopy → 500ms UI tick → parse output → update stats row → on exit: completion summary + post-actions
- External dependencies: robocopy.exe (Windows built-in), ntdll.dll (NtSuspendProcess/NtResumeProcess), optional WinRT toast notifications

## Current State
- What is already working: PowerShell implementation 100% functional
- What is broken or missing: C# rewrite does not exist yet
- What remains uncertain: nothing — spec is authoritative

## Next Phase Justification
- Chosen phase: Phase 2 — Builder/Verifier
- Why this is the correct next step: Spec is complete and authoritative. All behaviors, regex patterns, data structures, and UI layout are documented. Implementation can proceed without further research.
- Why major rewrite is or is not justified: Rewrite IS justified — C# WPF provides native Win32 P/Invoke, proper threading (no runspace hack), MVVM data binding, and compiles to a signed standalone EXE
