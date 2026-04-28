# Windows Dev Engine

> **The one-stop shop for Windows C# application development.**
> Clone it once. Spin up production-quality WPF apps in minutes, not days.

---

## What Is This?

Windows Dev Engine is a complete, pre-built development platform for creating Windows desktop applications with C# and WPF. Everything you need to go from zero to a polished, running application is already here — architecture patterns, debug infrastructure, design standards, copy-paste templates, and a library of animated UI samples covering every control type and visual style.

There is no setup. There is no boilerplate to write. You clone this repo, copy a template folder, rename it, and you are building features on day one.

This repository was built by **Commander Maverick** for one purpose: eliminate the repetitive setup work that wastes the first hours of every new project. The design philosophy, project structure, debug tools, color system, animations, and control patterns are defined once here — and inherited by every app you build from it forever.

---

## Why This Exists

Every time you start a new Windows application from scratch, you waste time on the same things:

- Setting up MVVM correctly
- Wiring Serilog and structured logging
- Building a custom window chrome that actually looks good
- Making controls match your design system
- Remembering how to do a sliding panel or a donut chart
- Debugging things that could have been caught instantly with an overlay

This repo solves all of that. Once. The first app you build from this template takes minutes to scaffold. Every app after that is faster. You spend 100% of your time on the actual features that matter.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 |
| Runtime | .NET 8 |
| UI Framework | WPF (Windows Presentation Foundation) |
| Pattern | MVVM (Model-View-ViewModel) |
| Logging | Serilog with File + Debug + Console sinks |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Commands | RelayCommand (generic and non-generic) |
| Styling | ResourceDictionary-first (zero hardcoded values) |
| Window Chrome | Custom — WindowStyle=None, AllowsTransparency=True |
| Graphics | Pure XAML vector paths (no image files) |
| Animation | XAML Storyboards with easing functions |
| Testing | xUnit |

---

## Repository Structure

```
windows-dev-engine/
│
├── WindowsDevEngine.sln          ← Open this to see ALL 22 projects in Visual Studio
│
├── AGENT_README.md               ← Entry point for AI agents building apps with this engine
│
├── agent/                        ← Complete operating manual (read before writing any code)
│   ├── 00_MANIFESTO.md           ← Philosophy, non-negotiables, Commander's aesthetic
│   ├── 01_BUILD_RECIPE.md        ← Phase-by-phase build process from init to shipped
│   ├── 02_DESIGN_STANDARDS.md    ← Color system, typography, spacing, animation rules
│   ├── 03_DEBUG_FIRST.md         ← Debug infrastructure patterns and Serilog setup
│   ├── 04_ARCHITECTURE.md        ← Project structure, MVVM rules, DI, naming conventions
│   ├── 05_WORKFLOW.md            ← Daily loop, git strategy, performance budgets
│   └── 06_CHECKLIST.md           ← 7-gate quality checklist (foundation → final verification)
│
├── templates/
│   ├── BaseProject/              ← THE MAIN TEMPLATE — copy this to start any new app
│   │   ├── App.xaml / App.xaml.cs
│   │   ├── Assets/Themes/        ← Colors.xaml, Typography.xaml, Controls.xaml, Animations.xaml
│   │   ├── Core/Commands/        ← RelayCommand (generic + non-generic)
│   │   ├── Core/Services/        ← NavigationService with history + events
│   │   ├── ViewModels/Base/      ← ViewModelBase (INotifyPropertyChanged + SetProperty)
│   │   ├── Debug/                ← DiagnosticsOverlay (FPS, memory, uptime, log tail)
│   │   └── Views/MainWindow.xaml ← Full custom chrome: titlebar, sidebar, content frame, statusbar
│   │
│   └── pages/
│       ├── SplashScreen/         ← Animated loading splash with logo scale-in + progress fill
│       ├── LoginPage/            ← Auth form with validation states, focus effects, SSO button
│       └── AboutPage/            ← App info, version, credits, links panel
│
├── samples/
│   ├── animations/
│   │   ├── DragonOverCastle/     ← Dragon flies over castle at night (bezier path + wing flap)
│   │   ├── MaverickMach10/       ← F-18 at Mach 10 (afterburner, shock diamonds, sonic boom)
│   │   ├── WizardSpell/          ← Wizard casting spell (rotating runes, energy beam, sparks)
│   │   └── BombsAndExplosions/   ← Bombs drop and explode (gravity, fireball, shockwave, debris)
│   │
│   ├── progress/
│   │   ├── CountdownClock/       ← Countdown timer with color shifts and critical glow
│   │   ├── FillingBars/          ← 5 bar variants (gradient, segmented ammo, XP, download)
│   │   ├── SpinningWheels/       ← 5 spinner types (arc, dots orbit, double ring, pulse, gear)
│   │   └── PieProgress/          ← 4 donut gauges with ArcSegment math and percentage labels
│   │
│   ├── themes/
│   │   ├── DarkMilitary/         ← Primary design language — full mock app
│   │   ├── FontColorShowcase/    ← Type scale, color swatches, glow text variants
│   │   ├── CyberNeon/            ← Cyberpunk (scanline + grid overlays, animated scan bar)
│   │   ├── GoldCommand/          ← Military gold (5-stop gradient, decorative command seal)
│   │   ├── ArcaneWizard/         ← Fantasy wizard (Georgia fonts, spell cards, mana bar)
│   │   └── NeonSciFi/            ← Sci-Fi (ship systems readout, hull breach alerts)
│   │
│   └── controls/
│       ├── DropdownMenuDemo/     ← Custom ComboBox, context menus, popup notification panels
│       ├── PopupModalDemo/       ← Sliding panels, confirm/info/input modals, toast notifications
│       ├── ButtonGallery/        ← Every button style with inline code labels
│       └── EmojiShowcase/        ← Emoji in UI contexts, size scale, category grid tiles
│
└── debug/
    └── AdornerOverlay/           ← GridAdorner, BoundsAdorner, extension helpers
```

---

## Getting Started in Under 5 Minutes

### Prerequisites

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (Community is free) with the `.NET desktop development` workload, **or** VS Code with the C# Dev Kit extension

### Step 1 — Clone the repo

```bash
git clone https://github.com/maverickmackten-eng/windows-dev-engine.git
cd windows-dev-engine
```

### Step 2 — Open the solution

```bash
start WindowsDevEngine.sln
```

All 22 projects load in Visual Studio. Right-click any sample in Solution Explorer and hit **Set as Startup Project**, then press F5. You're running.

### Step 3 — Start your own app

```bash
# Copy the base template to your project location
xcopy templates\BaseProject\ C:\Projects\MyNewApp\ /E /I

# Rename the project files
# BaseApp.csproj → MyNewApp.csproj
# Update namespace in App.xaml, App.xaml.cs, ViewModels, Views

# Open your new project
cd C:\Projects\MyNewApp
dotnet run
```

Your new app starts with:
- Custom dark window chrome (title bar, minimize, close, drag-to-move)
- Sidebar navigation ready to populate
- Serilog writing structured logs to `%LOCALAPPDATA%\MyNewApp\logs\`
- Global exception handler wired (nothing crashes silently)
- DiagnosticsOverlay ready on `Ctrl+Shift+D`
- Full ResourceDictionary design system (colors, typography, controls, animations)
- NavigationService with history and events
- RelayCommand for all user actions
- ViewModelBase with SetProperty and INotifyPropertyChanged

**That is what you get before writing a single line of feature code.**

---

## The Base Template — What You Actually Get

When you copy `templates/BaseProject/` you inherit a production-ready foundation:

### App Startup (`App.xaml.cs`)

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    ConfigureLogging();   // Serilog: File + Debug sinks, rolling daily, 7-day retention
    WireExceptionHandlers();  // DispatcherUnhandledException + AppDomain + TaskScheduler
    ConfigureServices();  // DI container: INavigationService, ViewModels
    base.OnStartup(e);
}
```

Three lines of infrastructure that every serious app needs — already written, already tested.

### Design System (`Assets/Themes/`)

All values live in ResourceDictionary files. **Nothing is hardcoded anywhere in the codebase.**

**Colors.xaml** — 20 semantic color keys:
```xml
BackgroundDeep    #0A0D12    <!-- Near-black main background -->
BackgroundPanel   #12171F    <!-- Cards, panels, sidebars -->
BackgroundElevated #1C2333   <!-- Hover states, elevated surfaces -->
AccentPrimary     #00B4FF    <!-- Electric blue — primary actions -->
AccentSecondary   #FF6B35    <!-- Tactical orange — warnings -->
AccentSuccess     #00FF94    <!-- Mission green — success states -->
AccentDanger      #FF2D55    <!-- Red alert — errors, destructive actions -->
AccentGold        #FFD700    <!-- Gold — achievements, highlights -->
TextPrimary       #FFFFFF
TextSecondary     #8899AA
TextDisabled      #445566
```

**Typography.xaml** — Font scale with named keys (XS through Hero/72pt), mono stack, display stack.

**Controls.xaml** — Pre-styled ButtonPrimary, ButtonSecondary, ButtonDanger, ButtonGhost, InputField, Card, Divider — all with correct hover/focus/press states and glow effects.

**Animations.xaml** — Ready-to-use storyboard resources: FadeIn, FadeOut, SlideInFromLeft, SlideInFromBottom, ScaleIn, Pulse, Shake, GlowPulse, SpinForever.

### DiagnosticsOverlay

Press `Ctrl+Shift+D` in any app built from this template. A dark overlay panel appears showing:

```
FPS: 60      ← Color-coded: green ≥55, yellow ≥30, red <30
Memory: 47 MB
Uptime: 0:04:32
View: DashboardPage
Last log: [14:22:01 INF] NavigateTo: Dashboard
─────────────────────────────
Ctrl+Shift+D  Toggle overlay
Ctrl+Shift+L  Open log file
Ctrl+Shift+S  Dump state
Ctrl+Shift+R  Restart app
```

This overlay costs nothing to keep running. It is collapsed by default in production, visible to developers in debug sessions. Every app that ships from this engine has this capability wired in.

### Custom Window Chrome

```xml
<Window WindowStyle="None" AllowsTransparency="True" Background="Transparent">
    <Border CornerRadius="12" Background="{StaticResource BackgroundDeep}">
        <!-- Titlebar with DragMove, minimize, close -->
        <!-- Sidebar with NavigationService -->
        <!-- Content Frame -->
        <!-- DiagnosticsOverlay (collapsed by default) -->
        <!-- Status bar -->
    </Border>
</Window>
```

No system title bar. No default WPF chrome. A sharp, custom window that looks exactly how you design it.

---

## Animation Samples

Every animation sample is pure XAML — no image files, no external dependencies, no GIFs. Everything is drawn with Path, Ellipse, Polygon, and Rectangle elements with WPF Storyboard animations.

### Dragon Over Castle

A full scene: star field, glowing moon with radial gradient, three parallax cloud layers moving at different speeds, a detailed castle silhouette with glowing windows, and a dragon that flies across the screen with wing-flap animation, vertical bobbing, and fire breath that activates at specific keyframes.

```
Dragon body: bezier Path with RadialGradientBrush fill
Wing flap: Canvas.Top oscillation on left/right wing canvases
Fire breath: LinearGradientBrush Path with opacity keyframes
Cloud parallax: 45s/35s/25s speeds for depth illusion
Full animation loop: 14 seconds, RepeatBehavior=Forever
```

### Commander Maverick at Mach 10

An F-18 fighter jet blasts across the screen from left to right with ExponentialEase EaseIn acceleration. Afterburner glows with a RadialGradientBrush. Five shock diamonds pulse behind the engines. The sonic boom ring expands from 2px to 200px as the jet breaks the sound barrier. Cloud streaks fly past in the opposite direction.

```
Jet transit: -300 to 1300px in 3 seconds (ExponentialEase)
Afterburner: RadialGradientBrush yellow→orange→transparent
Sonic boom: Width 2→200, Opacity 0→0.8→0, expanding ellipse
Cloud streaks: 0.9–1.5s right-to-left opposing motion
```

### Wizard Casting Spell

A magic circle rotates on the floor with rune TextBlocks spinning around it. Two concentric arcs rotate in opposite directions at different speeds. An energy beam flickers in opacity and width. Eight small sparks orbit the center point on a 3-second loop. A wizard silhouette stands at the edge, staff raised.

```
arcCircle1: 360° rotation in 8s (clockwise, dashed stroke)
arcCircle2: 360° rotation in 5s (counter-clockwise)
Sparks: 8 ellipses orbiting via Canvas.Left/Top keyframes
Energy beam: LinearGradientBrush, width + opacity flicker
```

### Bombs and Explosions

Two bombs drop from the top of the screen with realistic gravity (SplineDoubleKeyFrame for acceleration). On impact: a white flash, a fireball that grows from 0 to 160px diameter with DropShadowEffect glow, a shockwave ring that expands to 300px and fades, smoke that rises, and six debris particles scatter outward from the impact point.

```
Gravity: SplineDoubleKeyFrame (slow start, fast finish)
Fireball: RadialGradientBrush, scales 0→160px with glow
Shockwave: Ellipse stroke expands 0→300px, opacity 1→0
Stagger: bomb2 impacts 1 second after bomb1
```

---

## Progress Indicator Samples

### Countdown Clock

A large digital countdown with a DispatcherTimer. As time decreases: blue above 5 minutes, yellow inside the final minute, red in the final 10 seconds with the display glow kicking in (DropShadowEffect BlurRadius jumps to 30). A progress bar beneath the digits fills as time elapses. START, PAUSE, and RESET are all wired.

### Filling Bars

Five distinct bar styles in one demo:

| Bar | Style | Use Case |
|---|---|---|
| Mission Objective | Blue gradient + glow | Primary progress |
| Hull Integrity | Solid red | Damage/health |
| XP Progress | Gold 3-stop gradient | Reward systems |
| Ammo Count | 10 segmented blocks (7 green, 3 empty) | Discrete resource |
| Download | Purple gradient | Transfer/loading |

All bars animate from 0 on window load with staggered BeginTime so they fill in sequence.

### Spinning Wheels

Five spinner archetypes:
- **Classic Arc** — Single ArcSegment quarter-circle rotating
- **Dots Orbit** — 8 dots of decreasing opacity arranged in a circle, rotating as a unit
- **Double Ring** — Outer ring clockwise, inner ring counter-clockwise, different speeds
- **Pulse Ring** — 3 concentric rings expanding outward with 0.5s offset between each
- **Gear** — Center circle + 4 cardinal + 4 diagonal rectangles forming a mechanical gear

### Donut / Pie Gauges

Four donut gauges using WPF ArcSegment with manually calculated endpoint coordinates. Each gauge has a dark track ellipse behind it and a colored progress path on top with round StrokeStartLineCap and StrokeEndLineCap. A percentage TextBlock sits in the center.

```xml
<!-- Arc endpoint math for any percentage value P -->
double angle = (P / 100.0) * 360 - 90;  // -90 starts at top
double x = cx + r * Math.Cos(angle * Math.PI / 180);
double y = cy + r * Math.Sin(angle * Math.PI / 180);
```

---

## Theme Showcases

### Dark Military (Primary Design Language)

The default aesthetic for all apps built with this engine. Dark blue-grey backgrounds, electric blue primary accent, tactical orange for warnings, mission green for success, red alert for errors. A full mock application is shown with sidebar navigation, four stat cards, a data table with status badges, a complete button row (primary/secondary/danger/ghost), input fields in all states (normal/focused/error), and a status bar with version info.

### CyberNeon

Deep black background (`#04040C`). Cyan, hot pink, green, and yellow accents. Two DrawingBrush overlays are layered on the background: a scanline pattern (1×4 viewport, TileMode=Tile) and a 40×40 grid of faint lines. An animated scan bar (thin cyan rectangle) sweeps vertically with SineEase AutoReverse at a 4-second cycle. Neon glow effects on all text using DropShadowEffect.

### Gold Command

Military command room aesthetic. Near-black background (`#0A0700`) with gold (`#FFD700`) throughout. A 5-stop LinearGradientBrush creates the signature gold shimmer. Window border uses a LinearGradientBrush from dark gold to bright gold and back. A decorative seal (nested concentric ellipses with a ✦ center) reinforces the authority theme. Stat card borders use the same gold gradient.

### Arcane Wizard

Fantasy book aesthetic. Deep purple-black background. Georgia typeface throughout (not Segoe UI). Purple (`#AA44FF`), gold, and teal accent colors. Left panel shows spell schools as a navigation list with an animated mana bar (purple fill over dark track). Main panel shows three spell cards side by side — Chain Lightning, Wish, Mirror Image — each with school/level/cast time/effect data and colored borders matching their school.

### Neon Sci-Fi

Star Trek inspired command station. Near-black background (`#020810`). Electric blue/teal/green/red. "NEXUS STATION" displayed in Impact font with cyan DropShadowEffect. Right panel shows a ship systems readout with color-coded status lines (TACT/ENGR/NAVC/WARN/COMM). Three stat cards show CREW/WARP CORE/HULL with appropriate color-coded values. A HULL BREACH warning is displayed in red to show error state presentation.

---

## Control Samples

### Dropdown Menu Demo

A fully custom-styled ComboBox with a dark background, accent border on open state, and styled ComboBoxItem with blue highlight on hover. A placeholder text state when nothing is selected. Two right-click context menu zones demonstrating the DarkMenu and DarkMenuItem styles. Two popup panels: a notification bell dropdown (showing 3 notifications with timestamps) and a user account dropdown (showing name, role, and action buttons).

### Popup Modal Demo

Three patterns for interrupting the user:

**Sliding Panels** — A left navigation drawer and a right details drawer, both animated with `TranslateTransform.X` using `CubicEase EaseOut` at 250ms. A dark `#AA000000` overlay covers the main content when modals are open. Clicking the overlay closes everything.

**Modal Dialogs** — Three modal types shown: a red-bordered confirm dialog ("This action cannot be undone"), a blue-bordered info dialog (mission briefing), and a green-bordered input dialog (coordinate entry with a styled TextBox).

**Toast Notifications** — Success and error toasts that appear, hold for 3 seconds, then fade out with a `DoubleAnimation` on the Opacity property.

### Button Gallery

Every button variant in the design system, displayed in cards with inline code labels showing exactly how each is built:

| Style | Background | Border | Use For |
|---|---|---|---|
| Primary Blue | Gradient #0088CC→#00B4FF | None | Main call to action |
| Primary Orange | Gradient #CC4400→#FF6B35 | None | Secondary primary action |
| Gold Command | Gradient gold shimmer | None | Authority/premium actions |
| Danger Solid | Solid #FF2D55 | None | Destructive, no going back |
| Danger Outlined | 10% red bg | Red | Caution-level destructive |
| Secondary | Dark bg | Subtle border | Supporting actions |
| Ghost | Transparent | None | Tertiary/inline actions |
| Link | No bg, underline | None | Inline navigation |
| Small Icon | 28×28 | Subtle | Toolbar, icon-only actions |
| Hero | Large padding, glow | None | Single primary CTA |
| Toggle | State-swapping bg/color | Matches state | On/off controls |
| Disabled | Opacity 0.5 | Faint | Unavailable actions |

### Emoji Showcase

Demonstrating `FontFamily="Segoe UI Emoji"` across all UI contexts. Emoji in tab bars, emoji in status bars as live status indicators, emoji in buttons for visual weight. A size scale showing the same rocket emoji at 12/16/24/36/48/64px. Category grids: Military & Tactical (12 emoji), Science & Tech (12 emoji), Status & Alerts (12 emoji), People & Roles (8 emoji).

---

## Page Templates

### Splash Screen

A 600×380 center-screen window with no chrome. On load, a 0.6-second window fade-in plays simultaneously with a logo panel that scales from 60% to 100% using BackEase for a subtle overshoot. A progress bar fills from 0 to 480px over 2.8 seconds. A glow pulse on the logo icon runs forever (BlurRadius 10→30 SineEase AutoReverse). Status text updates as each initialization stage completes. Version and copyright text are anchored bottom-right and bottom-left.

To wire it: update `OnLoaded` in MainWindow.xaml.cs with your actual initialization tasks, then show your MainWindow and close the splash at the end.

### Login Page

A 480×580 authentication form. The card fades in and slides up 20px from below on load (CubicEase EaseOut 0.5s). Username and password fields each have a `Border` wrapper that changes `BorderBrush` from subtle to `AccentPrimary` on focus — giving the active field an electric blue outline. A collapsible red error border appears below the password field for validation failures. A horizontal divider with "or" text leads to an SSO button alternative.

### About Page

A 560×520 info panel for any app. Square app icon with gradient background and glow. Large app name with letter-spacing. Version and build number. Descriptive text block. Three link buttons (License, Report Bug, GitHub). A clean info table showing developer, engine, framework, and copyright. A close button anchored to the bottom right.

---

## Debug Infrastructure

The engine's debug philosophy is simple: **wiring debug infrastructure is the first feature you build, not an afterthought.** Every app from this template launches with full observability before a single line of feature code exists.

### Serilog Setup

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithMachineName()
    .WriteTo.File(
        path: logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    .CreateLogger();
```

Structured, rolling daily logs. Seven days of history retained automatically. Every log line includes timestamp, level, and the message. Every error includes the full exception.

### Global Exception Handler

```csharp
// Three handlers covering every exception path in a WPF app
DispatcherUnhandledException += OnDispatcherException;          // UI thread
AppDomain.CurrentDomain.UnhandledException += OnDomainException; // Background threads
TaskScheduler.UnobservedTaskException += OnTaskException;        // async/await
```

No exception escapes unlogged. The app either recovers and logs the error, or terminates gracefully after writing the full stack trace.

### DiagnosticsOverlay Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+Shift+D` | Toggle DiagnosticsOverlay |
| `Ctrl+Shift+L` | Open log file in Explorer |
| `Ctrl+Shift+S` | Dump full application state to log |
| `Ctrl+Shift+R` | Restart the application |

### AdornerLayer Debug Utilities

Two adorner classes for layout debugging, wired via extension methods:

```csharp
// Draw a debug grid over any element
AdornerOverlayExtensions.ToggleGrid(myElement);

// Highlight bounds + margin of any element
AdornerOverlayExtensions.AddBounds(myElement);

// Clear all bounds highlights
AdornerOverlayExtensions.ClearAllBounds(rootElement);
```

`GridAdorner` draws a blue 8px grid with major lines every 40px. `BoundsAdorner` draws the actual bounding box, an orange dashed margin box, and a size label (e.g., `320 × 64`) in the bottom-left corner of the element.

---

## Architecture Rules

Every app built from this engine follows the same architecture. This means any developer (or AI agent) can pick up any project and understand it immediately.

### MVVM Layer Boundaries

```
User action
    ↓
View (XAML only — no logic)
    ↓
ViewModel (state + commands + Serilog logging)
    ↓
Service (navigation, data, external APIs)
    ↓
Model (pure C# data classes)
```

**Views** contain zero business logic. Only `DragMove()`, keyboard shortcut wiring, and debug overlay toggle belong in code-behind.

**ViewModels** contain all state as properties with `SetProperty()`. All user actions are `ICommand`. Every command logs its execution via Serilog.

**Services** are registered in the DI container and injected into ViewModels via constructor parameters.

**Models** are plain C# classes with no UI references.

### Naming Conventions

| Item | Convention | Example |
|---|---|---|
| Views | `PascalCase + View/Window/Page` | `DashboardPage.xaml` |
| ViewModels | Same name + `ViewModel` | `DashboardViewModel.cs` |
| Commands | Verb + noun + `Command` | `LaunchMissionCommand` |
| Services | `I` prefix for interface | `INavigationService` |
| Resources | Semantic names, no colors in names | `AccentPrimary`, not `Blue` |
| Brush keys | Same as color key + `Brush` | `AccentPrimaryBrush` |

---

## The Build Recipe

Every new app follows these six phases in order. Never skip phases. Never start phase N+1 until phase N is committed.

**Phase 0 — Initialize**
Copy `templates/BaseProject/`, rename files and namespaces, create git repo, make the first commit with only the base project. Confirm it compiles and runs.

**Phase 1 — Debug Infrastructure**
Verify Serilog writes to the log file on first run. Verify the DiagnosticsOverlay shows FPS and memory. Verify all four keyboard shortcuts work. Verify the global exception handler catches and logs a test exception. Commit: `debug: wire Serilog, DiagnosticsOverlay, exception handlers`.

**Phase 2 — App Shell**
Build the main window chrome, sidebar navigation, content frame, and status bar. Wire NavigationService. Add placeholder pages for each section. Confirm navigation works between every page. Commit: `shell: main window, nav, placeholder pages`.

**Phase 3 — Splash Screen**
Wire the splash screen from `templates/pages/SplashScreen/`. It shows during the 1–2 second startup initialization. Confirm it fades out and hands off to the main window cleanly.

**Phase 4 — Features (Repeat)**
For each feature: write the ViewModel first (with all logging), then the XAML, then wire commands. Check the 8-item per-feature checklist in `agent/06_CHECKLIST.md` before moving to the next feature.

**Phase 5 — Polish**
Apply consistent spacing using the scale. Verify hover/focus states on all interactive elements. Add animations where motion enhances understanding. Check performance: startup under 2 seconds, navigation under 100ms, 60 FPS during animations.

**Phase 6 — Final Gates**
Run the 8-item final verification checklist. Confirm logging is production-level (no debug noise). Confirm all keyboard shortcuts work. Confirm the About page has the correct version. Tag the commit.

---

## Working with AI Agents

This repository is designed to be handed directly to an AI coding agent. The `agent/` folder is the complete operating manual. When you start a new session, point the agent at `AGENT_README.md` first. It tells the agent what to read and in what order.

The agent documentation is written to answer every question an AI agent will ask during development:

- `00_MANIFESTO.md` — What aesthetic am I building toward? What are the non-negotiables?
- `01_BUILD_RECIPE.md` — What do I do first? What comes after that?
- `02_DESIGN_STANDARDS.md` — What hex color do I use for a warning? What font size is "body"?
- `03_DEBUG_FIRST.md` — How do I set up Serilog? What should I log? What shouldn't I log?
- `04_ARCHITECTURE.md` — Where does this file go? What layer does this logic belong in?
- `05_WORKFLOW.md` — How do I name this branch? What goes in the commit message?
- `06_CHECKLIST.md` — Am I done? What did I forget?

An agent with access to this repo and the operating manual can build a production-quality WPF application without asking clarifying questions about structure, style, or process.

---

## Performance Budgets

Every app built from this engine is expected to hit these targets:

| Metric | Budget | How to Verify |
|---|---|---|
| Cold startup time | < 2 seconds to first frame | Stopwatch from process launch |
| Page navigation | < 100ms | Log timestamps in NavigationService |
| Animation frame rate | 60 FPS sustained | DiagnosticsOverlay FPS counter |
| Memory at startup | < 100MB | DiagnosticsOverlay memory readout |
| Memory after 1 hour | < 150MB | Check for GC pressure / leaks |

---

## Commit Message Format

```
type: short description

Types:
  init     — First commit of a new project
  debug    — Debug infrastructure changes
  shell    — App chrome, navigation, window structure
  feat     — New feature
  fix      — Bug fix
  polish   — Visual improvements, spacing, animations
  perf     — Performance improvement
  docs     — Documentation
  refactor — Code restructuring (no behavior change)
```

Examples:
```
init: base project from windows-dev-engine template
debug: wire Serilog with rolling file sink and DiagnosticsOverlay
feat: add weapons system dashboard with live status indicators
fix: navigation back-stack not clearing on fresh navigate
polish: add slide-in animation to sidebar nav items
```

---

## Requirements

- **OS:** Windows 10 version 1903 or later, Windows 11
- **SDK:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **IDE:** Visual Studio 2022 (any edition) with `.NET desktop development` workload
- **RAM:** 4GB minimum, 8GB recommended for development
- **No external font installs required** — all fonts (Segoe UI, Cascadia Code, Segoe UI Emoji) are bundled with Windows 10/11

---

## What's in the NuGet Packages

The base template uses four packages. That's it. No UI component libraries. No third-party animation frameworks. No bloat.

```xml
<PackageReference Include="Serilog" Version="4.*"/>
<PackageReference Include="Serilog.Sinks.File" Version="6.*"/>
<PackageReference Include="Serilog.Sinks.Debug" Version="3.*"/>
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.*"/>
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.*"/>
```

Everything visual — every button style, every animation, every layout — is written in XAML using the WPF standard library. Zero runtime dependencies beyond .NET 8.

---

## License

MIT — use it, copy it, build on it. No attribution required.

---

## Built By

**Commander Maverick**
Windows Dev Engine v1.0.0
`NCC-1701-M`

> *"If it can't be debugged, it doesn't ship."*
