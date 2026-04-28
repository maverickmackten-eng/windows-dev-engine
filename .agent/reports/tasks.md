# Tasks — Mavericks-RoboCopy C# Rewrite

## Phase 2 — Core Engine

### TASK-001 — Project scaffold
- [ ] Create `src/MavericksRoboCopy/MavericksRoboCopy.csproj` (.NET 8, WPF, nullable, implicit usings)
- [ ] Add `src/MavericksRoboCopy.Tests/MavericksRoboCopy.Tests.csproj` (xUnit, Moq)
- [ ] Add `MavericksRoboCopy.sln`
- [ ] Verify `dotnet build` succeeds

### TASK-002 — SettingsManager
- [ ] `Models/AppSettings.cs` — 9 properties matching spec §2
- [ ] `Services/SettingsManager.cs` — Load (corrupt JSON → defaults), Save (UTF-8 BOM-less)
- [ ] Unit test: missing file → defaults; corrupt file → defaults; round-trip save/load

### TASK-003 — RoboCopyArgBuilder
- [ ] `Services/RoboCopyArgBuilder.cs` — static `Build(AppSettings s)` → `string[]`
- [ ] Implement all 14 conditional flags from spec §8
- [ ] Unit test: port all 14 Pester assertions from `Mavericks-RoboCopy.Tests.ps1`

### TASK-004 — RoboCopyProcess
- [ ] `Services/RoboCopyProcess.cs` — spawn robocopy, pump stdout/stderr into `ConcurrentQueue<string>` with `O:`/`E:` prefix
- [ ] Timer flush every 500 ms → raise `LineReceived` event
- [ ] `NtProcessHelper.cs` — P/Invoke NtSuspendProcess / NtResumeProcess (signatures from spec §16)
- [ ] Pause / Resume / Cancel methods
- [ ] Unit test: process exits cleanly; pause then resume; cancel mid-run

### TASK-005 — OutputParser
- [ ] `Services/OutputParser.cs` — all 4 stat-tracking regexes (spec §9)
- [ ] Raise typed events: `FileTransferred`, `NewDirectory`, `ErrorLine`, `SummaryLine`
- [ ] Size parser with k/m/g multipliers
- [ ] Unit test: feed sample robocopy output lines, assert correct events fired

### TASK-006 — StatsEngine
- [ ] `Services/StatsEngine.cs` — rolling 8-sample speed buffer, ETA formula from spec §17
- [ ] `FileTypeBreakdown` dictionary (ext → count/bytes)
- [ ] Post-completion top-15 type table
- [ ] Unit test: ETA formula with known values; breakdown dict accumulation

### TASK-007 — LogRouter
- [ ] `Services/LogRouter.cs` — 3-channel router (All / Errors / Summary)
- [ ] Color-code rules from spec §9 routing table
- [ ] Log file writer (per-run timestamp, spec §26 format)

## Phase 3 — WPF UI

### TASK-008 — MainViewModel
- [ ] `ViewModels/MainViewModel.cs` — INotifyPropertyChanged, all bindable props from spec §4
- [ ] Commands: StartCommand, PauseResumeCommand, CancelCommand, OpenSourceCommand, OpenDestCommand
- [ ] Wire ViewModel → all services

### TASK-009 — Dark/Flame Theme
- [ ] `Themes/DarkFlame.xaml` — colour palette from spec §28 (BgDeep #0A0D12, AccentPrimary #00B4FF, Flame #FF6B35, etc.)
- [ ] Button styles, ProgressBar style, TabItem style, TextBox style
- [ ] Apply in App.xaml

### TASK-010 — MainWindow
- [ ] `Views/MainWindow.xaml` — WindowStyle=None, custom title bar
- [ ] All controls from spec §4: $src, $dst, $mode, $opts, $filt, $post, $stats, $prog, $log, $btns
- [ ] Stats row (speed, ETA, files, bytes, folders, errors)
- [ ] Progress bar with percentage label
- [ ] 3-tab log panel
- [ ] File-type breakdown grid (post-completion)

### TASK-011 — ToastService
- [ ] `Services/ToastService.cs` — ObservableCollection<ToastItem>, auto-dismiss after 4s
- [ ] `Views/ToastHost.xaml` — positioned bottom-right, slide-up + fade animation

## Phase 4 — Advanced Features

### TASK-012 — PreScanService
- [ ] `Services/PreScanService.cs` — robocopy /L, parse pre-scan regexes (spec §19), return (totalFiles, totalBytes)
- [ ] Show pre-scan results in log before transfer starts

### TASK-013 — AutoBooster / WatchTransfers
- [ ] `Services/AutoBooster.cs` — FileSystemWatcher on Desktop/Documents/Downloads + removable drives
- [ ] 2-poll confirmation logic (spec §25)
- [ ] Win32 EnumWindows window detection
- [ ] Perf counter threshold
- [ ] Toast notification on boost

### TASK-014 — ConsoleRunner
- [ ] `ConsoleRunner/Program.cs` — CLI entry, same ArgBuilder, no UI, stdout output, spec §28 differences

## Phase 5 — Tests, Polish, Release

### TASK-015 — Full xUnit test suite
- [ ] Port all 14 Pester tests
- [ ] Add OutputParser unit tests (all 4 regexes + size parser)
- [ ] Add StatsEngine ETA tests
- [ ] Add integration test: dry-run against temp dir, assert exit code 0/7

### TASK-016 — GitHub Actions CI
- [ ] `.github/workflows/build.yml` — dotnet build + test on push

### TASK-017 — Installer
- [ ] MSIX or WiX installer
- [ ] Register AutoBooster as scheduled task on install

### TASK-018 — Documentation
- [ ] README.md with screenshots
- [ ] CHANGELOG.md
- [ ] Architecture diagram
