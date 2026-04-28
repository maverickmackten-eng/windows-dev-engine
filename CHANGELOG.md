# Changelog

All notable changes to Windows Dev Engine are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

### Planned
- `templates/` scaffold directory with ready-to-copy page and control templates
- `apps/MavericksCopy` — full C# rewrite of Mavericks-RoboCopy using the engine
- NuGet package: `WindowsDevEngine.Core`
- Theming: `WarmOrange.xaml` and `HighContrast.xaml` variants
- Unit test project for shared ViewModelBase / RelayCommand

---

## [0.5.0] — 2026-04-28

### Added — Step 12: Final polish
- `README.md` — full project overview, repo structure tree, quickstart, contribution guide
- `CHANGELOG.md` — this file
- `.gitignore` — covers build outputs, runtime files, IDE artefacts

## [0.4.0] — 2026-04-28

### Added — Step 11: CI/CD
- `.github/workflows/build.yml`
  - **Job 1:** Matrix build of all 6 sample projects (Debug + Release) on `windows-latest`
  - **Job 2:** Full solution build via `WindowsDevEngine.sln` with artifact upload
  - **Job 3:** Lint check (CRLF + BOM) on `ubuntu-latest`
  - Triggers on push to `main`, `dev`, `feature/**`, `fix/**` and all PRs targeting `main`

## [0.3.0] — 2026-04-28

### Added — Step 10: Root solution + build props
- `WindowsDevEngine.sln` — all 6 sample projects registered with solution folder grouping
- `Directory.Build.props` — shared `<LangVersion>latest`, `<Nullable>enable`,
  `<ImplicitUsings>enable`, `<Deterministic>true`, `Microsoft.CodeAnalysis.NetAnalyzers`

## [0.2.0] — 2026-04-28

### Added — Steps 7–9: Controls, Layout, Docs

#### Step 7 — `samples/controls/`
- **DropdownMenuDemo** — styled `ComboBox` (animated chevron), popup context menu (icon + hint),
  cascading submenu (right-spawn)
- **PopupModalDemo** — confirm dialog (scale-in, Esc=cancel), form modal (validation),
  detail drawer (slide from right)
- **ToastDemo** — 4 types (Info / Success / Warning / Error), auto-dismiss timer,
  sticky mode, top/bottom-right stacking, per-toast close button

#### Step 8 — `samples/layout/`
- **NavigationShellDemo** — fixed 200px sidebar, 5 nav items with active accent-bar highlight,
  fade + translate page swap (150ms), collapse to 52px icon-only mode
- **DataGridDemo** — dark `DataGrid`, real-time `ICollectionView` filter, status badge column
  (DataTrigger-driven), multi-select, add / delete rows, live row / selection count status bar
- **SplitPaneDemo** — 3 layouts: horizontal / vertical / triple split — styled `GridSplitter`
  with hover accent highlight + double-click reset to equal size

#### Step 9 — `docs/`
- **GETTING-STARTED.md** — 5-step scaffold guide with full code snippets
- **ARCHITECTURE.md** — `ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`, `SettingsService`
  (atomic JSON write), `NavigationService`, DI cheat sheet, global exception handlers,
  unit test pattern
- **THEMING.md** — complete 36-key `ResourceDictionary` reference, minimal theme template,
  runtime switching code, custom theme creation steps
- **CONTROLS.md** — 11-control reference with XAML usage, data interfaces, code patterns

## [0.1.0] — 2026-04-27

### Added — Steps 1–6: Foundation + Themes
- Repository initialized (`maverickmackten-eng/windows-dev-engine`)
- Agent workspace scaffolded: `agent-workspace/` with knowledge base, session intake,
  builder-verifier, repo-analyst, research-planner templates and Python tooling
- **Step 6: Themes** — `DarkMilitary.xaml` ResourceDictionary with full color palette,
  typography scale, and dimension tokens
- `samples/` directory structure established
- Serilog added as the standard logging dependency across all sample projects
