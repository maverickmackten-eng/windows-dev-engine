# Stack Decisions

## Decision
- Project: Mavericks-RoboCopy
- Existing stack: PowerShell 5.1, WinForms (via Mav-AppTemplate.psm1 custom framework), .NET Framework
- Chosen language strategy: **C# 12 / WPF / .NET 8** — full rewrite
- Why: Native P/Invoke for NtSuspendProcess/NtResumeProcess; real async/await instead of runspace hack; XAML data binding; single self-contained EXE; proper xUnit test coverage; faster startup
- Constraints: Windows-only (robocopy.exe dependency); must match existing UX exactly per spec
- Re-evaluate when: if MAUI achieves parity with WPF Win32 interop
