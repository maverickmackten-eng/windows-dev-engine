# Router State

## Current Phase
- Phase: **Phase 2 — Builder/Verifier** (active implementation)
- Why this phase was selected: Repo analysis (Phase 1) is complete via the 830-line PS1 deep-read. Technical spec is documented in `mavericks-robocopy-spec.md`. Plan and tasks exist. Implementation has not yet started.
- Required child skill: `builder-verifier.agent.md`

## Language Strategy
- Decision: **C# / WPF / .NET 8** — full rewrite from PowerShell + WinForms/custom framework
- Reasoning: Spec requires NtSuspendProcess P/Invoke, ConcurrentQueue background thread, ObservableCollection toasts, XAML data-binding, Win32 EnumWindows — all native C#. The PS module Mav-AppTemplate already mirrors WinForms patterns that map 1:1 to WPF MVVM.

## Next Transition
- Exit criteria for current phase: All §1–§30 spec sections have corresponding implemented and tested C# classes; Pester tests ported to xUnit; robocopy integration test passes with exit-code 0 (already-in-sync).
- Next likely phase: Phase 3 — Polish / Release (installer, auto-update, signed binary)
