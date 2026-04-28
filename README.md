# Windows Dev Engine

> A dark-themed WPF component library, sample gallery, and scaffold system for
> building professional Windows desktop apps in C# / .NET 8.

[![Build](https://github.com/maverickmackten-eng/windows-dev-engine/actions/workflows/build.yml/badge.svg)](https://github.com/maverickmackten-eng/windows-dev-engine/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![WPF](https://img.shields.io/badge/WPF-Windows%20only-0078D4?logo=windows)](https://learn.microsoft.com/dotnet/desktop/wpf)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## What Is This?

Windows Dev Engine is a living reference repo that provides:

| What | Where |
|---|---|
| Runnable WPF control samples | `samples/controls/` |
| Runnable WPF layout samples | `samples/layout/` |
| Dark theme `ResourceDictionary` | (to be added to `Themes/`) |
| Architecture patterns (MVVM, DI, Settings) | `docs/ARCHITECTURE.md` |
| Full theming guide | `docs/THEMING.md` |
| Per-control usage reference | `docs/CONTROLS.md` |
| Getting started scaffold guide | `docs/GETTING-STARTED.md` |

The primary consumer of this engine is **Mavericks-RoboCopy C#** — a full rewrite
of the PowerShell robocopy GUI using these patterns and components.

---

## Repository Structure

```
windows-dev-engine/
├── .github/
│   └── workflows/
│       └── build.yml              ← CI: build all samples on push/PR
├── agent-workspace/           ← AI agent session files, knowledge base
├── docs/
│   ├── GETTING-STARTED.md     ← scaffold a new app in 5 steps
│   ├── ARCHITECTURE.md        ← MVVM, DI, ViewModelBase, SettingsService
│   ├── THEMING.md             ← all ResourceDictionary keys, runtime switching
│   └── CONTROLS.md            ← per-control XAML usage + data interface
├── samples/
│   ├── controls/
│   │   ├── DropdownMenuDemo/  ← ComboBox, popup menu, cascading submenu
│   │   ├── PopupModalDemo/    ← confirm dialog, form modal, detail drawer
│   │   └── ToastDemo/         ← 4-type toasts, auto-dismiss, sticky, stacking
│   └── layout/
│       ├── NavigationShellDemo/ ← sidebar nav, page-swap animation, collapse
│       ├── DataGridDemo/      ← dark DataGrid, filter, badges, add/delete
│       └── SplitPaneDemo/     ← H/V/triple split, styled GridSplitter
├── Directory.Build.props      ← shared build settings for all projects
├── WindowsDevEngine.sln       ← open this in Visual Studio
├── CHANGELOG.md
└── README.md
```

---

## Quickstart

### Prerequisites
- Windows 10 1903+ or Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 17.8+ **or** VS Code with C# Dev Kit

### Run any sample

```bash
git clone https://github.com/maverickmackten-eng/windows-dev-engine.git
cd windows-dev-engine

# Run a specific sample
dotnet run --project samples/controls/ToastDemo
dotnet run --project samples/layout/NavigationShellDemo
dotnet run --project samples/layout/DataGridDemo
```

### Open the full solution

```bash
start WindowsDevEngine.sln
# or: devenv WindowsDevEngine.sln
```

All projects appear under **Solution Explorer → samples → controls / layout**.

---

## Sample Gallery

| Sample | Category | Key Patterns |
|---|---|---|
| **DropdownMenuDemo** | Controls | Styled `ComboBox`, popup context menu, cascading submenu |
| **PopupModalDemo** | Controls | Scale-in confirm dialog, form modal with validation, side drawer |
| **ToastDemo** | Controls | 4-type toasts, auto-dismiss, sticky, bottom-right stacking |
| **NavigationShellDemo** | Layout | Sidebar nav + page-swap animation + collapse toggle |
| **DataGridDemo** | Layout | Dark `DataGrid`, live filter, status badges, add/delete rows |
| **SplitPaneDemo** | Layout | H / V / triple split panes with hover-accent `GridSplitter` |

> **Screenshots:** _Coming soon — add to `docs/screenshots/` and reference here._

---

## Architecture in 30 Seconds

```
View (XAML)  ←→  ViewModel (: ViewModelBase)  →  Service (SettingsService, etc.)
                   └─ RelayCommand / AsyncRelayCommand
                   └─ SetField() + OnPropertyChanged()
                   └─ RunAsync() guard (IsLoading / ErrorMessage)
```

Full detail: **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**

---

## Theming in 30 Seconds

1. Every color is a named `ResourceDictionary` key (`Accent`, `BgDeep`, `TextPrimary`, …)
2. All controls use `{DynamicResource}` — never hardcoded hex
3. Switch themes at runtime by swapping the merged dictionary

Full key reference: **[docs/THEMING.md](docs/THEMING.md)**

---

## CI / CD

Every push and pull request runs three jobs:

| Job | What it does | Runner |
|---|---|---|
| **Build (matrix)** | Builds each of the 6 samples in Debug + Release | `windows-latest` |
| **Build solution** | Builds `WindowsDevEngine.sln` end-to-end, uploads `.exe` artifacts | `windows-latest` |
| **Lint** | Checks CRLF line endings and UTF-8 BOM in source files | `ubuntu-latest` |

Badge status live at the top of this file.

---

## Contributing

1. Fork the repo
2. Create a feature branch: `git checkout -b feature/my-control`
3. Follow the patterns in an existing sample (same XAML structure, same theme keys)
4. Add your project to `WindowsDevEngine.sln`
5. Open a PR targeting `main` — CI must be green

**Coding standards:**
- No hardcoded colors — use `{DynamicResource}` keys from the theme
- No `System.Windows` references in ViewModels
- All new samples must build in Release with zero errors
- Serilog for all logging (`Log.Debug`, `Log.Information`, `Log.Error`)

---

## License

MIT — see [LICENSE](LICENSE) file.

---

<sub>Built with ⚡ by Commander Maverick — Montréal, QC</sub>
