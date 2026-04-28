# Session Intake

## Startup Checklist
- Read existing master blueprint: ✅ `mavericks-robocopy-spec.md` (30 sections, 1041 lines)
- Read existing project report: ✅ `Mavericks-RoboCopy.ps1` (830 lines), `Mav-AppTemplate.psm1` (~2800 lines)
- Read existing path index: ✅ see Path Index below
- Read current progress: ✅ Phase 1 complete — spec generated
- Read relevant knowledge files: ✅ WPF, NtSuspendProcess, RoboCopy output parsing
- Read active code files for this session: PowerShell source files under `/home/user/Mavericks-RoboCopy/`

## Session Goal
- Requested outcome: Full C# WPF rewrite of the Mavericks-RoboCopy PowerShell application
- Active area of the project: `src/MavericksRoboCopy/` (to be created)
- Likely current phase: Phase 2 — Builder/Verifier
- Reason: Spec is complete, analysis done, ready for implementation

## Required Reading Evidence
- Files opened this session:
  - `/home/user/Mavericks-RoboCopy/Mavericks-RoboCopy.ps1`
  - `/home/user/Mavericks-RoboCopy/Mav-AppTemplate.psm1`
  - `/home/user/Mavericks-RoboCopy/tools/Console-Run.ps1`
  - `/home/user/Mavericks-RoboCopy/tools/Watch-Transfers.ps1`
  - `/home/user/Mavericks-RoboCopy/tools/Reset-State.ps1`
  - `/home/user/Mavericks-RoboCopy/tests/Mavericks-RoboCopy.Tests.ps1`
- Entrypoints reviewed: `Mavericks-RoboCopy.ps1` — main GUI entry; `Console-Run.ps1` — CLI entry
- Configs reviewed: `settings.json` (9 keys, JSON, `%APPDATA%\MavericksRoboCopy\`)
- Tests reviewed: `Mavericks-RoboCopy.Tests.ps1` — 14 Pester assertions
- Unknowns still open: none — spec fully covers all behaviors
