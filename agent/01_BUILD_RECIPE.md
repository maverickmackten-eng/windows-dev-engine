# 01 — The Build Recipe

## From Idea to Shipped App — Follow In Order

This is the only process. Do not skip steps. Do not reorder steps.

---

## Phase 0: Project Initialization (Before Any Code)

```
[ ] 1. Copy templates/BaseProject/ to a new folder with the app name
[ ] 2. Rename the solution, project, and namespace throughout:
        - Find: "BaseApp"
        - Replace with: "YourAppName"
[ ] 3. Open in Visual Studio, verify it builds and runs (empty shell is fine)
[ ] 4. Initialize git: git init, git add ., git commit -m "init: project scaffold"
[ ] 5. Read agent/04_ARCHITECTURE.md to confirm folder structure is correct
```

---

## Phase 1: Debug Infrastructure (Before Any Feature)

```
[ ] 1. Install NuGet packages:
        - Serilog
        - Serilog.Sinks.File
        - Serilog.Sinks.Debug
        - Serilog.Enrichers.Environment
[ ] 2. Copy debug/LoggingTemplate/ → wire into App.xaml.cs
[ ] 3. Copy debug/DiagnosticsOverlay/ → add to MainWindow as collapsed overlay
[ ] 4. Add keyboard shortcut: Ctrl+Shift+D toggles DiagnosticsOverlay visibility
[ ] 5. Verify: Run app, press Ctrl+Shift+D, see FPS counter and memory usage
[ ] 6. Verify: Check logs/ folder for log file after first run
[ ] 7. git commit -m "debug: wire logging and diagnostics overlay"
```

---

## Phase 2: Shell & Navigation

```
[ ] 1. Copy the appropriate layout from templates/layouts/ for your app
[ ] 2. Set up navigation service in ViewModels/
[ ] 3. Add placeholder views for each page you will build
[ ] 4. Wire navigation so all pages are reachable (empty pages are fine)
[ ] 5. Apply the base theme from samples/themes/DarkMilitary/ (or chosen theme)
[ ] 6. Verify: Can navigate to every page without crash
[ ] 7. git commit -m "shell: navigation and layout scaffold"
```

---

## Phase 3: Splash Screen

```
[ ] 1. Copy templates/pages/SplashScreen/ 
[ ] 2. Add your app name, version, and loading animation
[ ] 3. Wire to App.xaml.cs startup sequence (show splash, init, dismiss)
[ ] 4. Minimum display time: 2 seconds (prevents flash on fast machines)
[ ] 5. git commit -m "feat: splash screen"
```

---

## Phase 4: Feature Development (Repeat Per Feature)

For each feature, follow this loop:

```
[ ] 1. Define the ViewModel first — properties, commands, no UI yet
[ ] 2. Write unit tests for ViewModel logic (if testable)
[ ] 3. Build the View (XAML) and bind to ViewModel
[ ] 4. Add log statements at every state change in the ViewModel
[ ] 5. Add error handling: try/catch with Serilog.Log.Error(ex, "context message")
[ ] 6. Add user-visible error state to ViewModel (ErrorMessage property)
[ ] 7. Test the golden path
[ ] 8. Test the failure paths (network down, bad input, empty state)
[ ] 9. git commit -m "feat: [feature name]"
```

---

## Phase 5: Visual Polish

```
[ ] 1. Review every page against agent/02_DESIGN_STANDARDS.md
[ ] 2. Add any animations from samples/animations/ that enhance the UX
[ ] 3. Add progress indicators from samples/progress/ wherever async work happens
[ ] 4. Check all fonts, colors, and spacing match ResourceDictionary values
[ ] 5. Test at 1920x1080, 2560x1440, and 1366x768 (common resolutions)
[ ] 6. git commit -m "polish: visual refinements"
```

---

## Phase 6: Final Gates (Before Calling It Done)

Run through `agent/06_CHECKLIST.md` completely.
Every unchecked box is a reason not to ship.

---

## Commit Message Format

```
type: short description

Types:
  init    - project scaffold
  debug   - debug/logging infrastructure
  shell   - navigation, layout, base structure
  feat    - new feature
  fix     - bug fix
  polish  - visual improvements
  perf    - performance improvement
  docs    - documentation only
  refactor - code restructure, no behavior change
```

---

## File Creation Order Within A Feature

Always in this order:

```
1. Models/FeatureName.cs          (data structures)
2. ViewModels/FeatureViewModel.cs  (logic, commands, state)
3. Views/FeatureView.xaml          (UI, bindings)
4. Views/FeatureView.xaml.cs       (minimal code-behind, event-to-command wiring)
```

Never create a View before its ViewModel. Never put logic in code-behind.
