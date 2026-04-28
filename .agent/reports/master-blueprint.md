# Master Blueprint — Mavericks-RoboCopy C# Rewrite

## Mission
- Project goal: Full C# 12 / WPF / .NET 8 rewrite of the Mavericks-RoboCopy PowerShell GUI
- Success criteria: Feature-parity with PS1 source; all 14 Pester tests ported to xUnit and passing; robocopy integration test exits with code 0
- Non-goals: New features beyond what exists in the PS1 source; cloud sync; non-Windows support

## Architecture
- Major components:
  1. `SettingsManager` — JSON serialise/deserialise, graceful corrupt-file fallback
  2. `RoboCopyArgBuilder` — pure static class, 9 settings → robocopy CLI string
  3. `RoboCopyProcess` — spawn, ConcurrentQueue<string> stdout/stderr pump, pause/resume via ntdll P/Invoke
  4. `OutputParser` — regex pipeline, raises typed events (FileTransferred, NewDir, Error, Summary)
  5. `StatsEngine` — rolling 8-sample speed buffer, ETA calc, file-type breakdown dictionary
  6. `LogRouter` — routes colored lines to 3 tab targets (All, Errors, Summary)
  7. `PreScanService` — robocopy /L mode, returns (totalFiles, totalBytes)
  8. `AutoBooster` — FileSystemWatcher on multiple roots + Win32 EnumWindows window detection
  9. `MainViewModel` — INotifyPropertyChanged, all bindable state
  10. `MainWindow.xaml` — WPF, WindowStyle=None, matches Mav-AppTemplate dark/flame theme
  11. `ToastService` — ObservableCollection<ToastItem> bottom-right stack
  12. `ConsoleRunner` — CLI entry point, same arg builder, no UI
- Data flow: UI → ViewModel → ArgBuilder → Process → OutputParser → StatsEngine → ViewModel → UI
- Startup order: App.xaml.cs → SettingsManager.Load() → MainViewModel() → MainWindow.Show()

## Runtime Map
- Services: AutoBooster runs as scheduled task on user logon (separate EXE or background thread)
- Processes: MavericksRoboCopy.exe (WPF), robocopy.exe (child), optional MavericksAutoBoost.exe
- Path and config contracts:
  - Settings: `%APPDATA%\MavericksRoboCopy\settings.json`
  - Log files: `%APPDATA%\MavericksRoboCopy\Logs\YYYY-MM-DD_HH-mm-ss.log`
  - Auto-log: `%APPDATA%\MavericksRoboCopy\auto-booster.log`
  - Recent paths: `%APPDATA%\MavericksRoboCopy\recent-paths.json`

## Stack Strategy
- Chosen: C# 12, .NET 8, WPF, MVVM (no framework — lightweight hand-rolled INotifyPropertyChanged)
- Why: Native Win32 P/Invoke, real threading, XAML styling, single EXE publish
- Rejected: WinForms (no XAML), MAUI (overkill), Avalonia (not native Win32)

## Delivery Plan
- Phase 1: ✅ Spec / Analysis complete
- Phase 2: Core engine (SettingsManager, ArgBuilder, RoboCopyProcess, OutputParser, StatsEngine)
- Phase 3: WPF UI (MainWindow, all controls matching spec §4, dark/flame theme)
- Phase 4: Advanced features (AutoBooster, WatchTransfers, ConsoleRunner, ToastService)
- Phase 5: Tests, installer (WiX or MSIX), signed binary, README

## Verification Plan
- Unit checks: xUnit tests for ArgBuilder (14 cases from Pester), OutputParser (all regex), StatsEngine (ETA formula), SettingsManager (corrupt JSON fallback)
- Integration checks: robocopy /L dry-run against a temp directory, assert exit code 0
- Startup checks: App cold-start with missing settings.json → defaults applied
- UI checks: All controls enabled/disabled correctly per spec §4 initial states
- Regression: Re-run xUnit on every push via GitHub Actions

## Risks
- NtSuspendProcess not documented — use exact P/Invoke signatures from spec §16
- robocopy output encoding may vary on non-EN Windows — use UTF-8 with OEM fallback
- AutoBooster Win32 EnumWindows callback must be static delegate — document in code
