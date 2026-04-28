# 06 — Build Checklist

## Use This Before Calling Anything "Done"

Run through every gate. A box left unchecked means the build is not done.

---

## Gate 1: Project Foundation

```
[ ] Project compiles with zero errors
[ ] Project compiles with zero warnings (or all warnings are intentional/documented)
[ ] Folder structure matches agent/04_ARCHITECTURE.md exactly
[ ] All placeholder names ("BaseApp", "YourAppName") replaced with actual app name
[ ] .gitignore is present and covers: bin/, obj/, .vs/, *.user, logs/
[ ] Git initialized, first commit made
```

---

## Gate 2: Debug Infrastructure

```
[ ] Serilog wired in App.xaml.cs (file sink + debug sink)
[ ] Log file created in LocalAppData/AppName/logs/ on first run
[ ] GlobalExceptionHandler wired — tested by temporarily throwing an exception
[ ] DiagnosticsOverlay present and accessible via Ctrl+Shift+D
[ ] FPS counter visible and updating in overlay
[ ] Memory usage visible in overlay
[ ] Ctrl+Shift+L opens log file
[ ] Ctrl+Shift+S dumps ViewModel state to log
```

---

## Gate 3: Application Shell

```
[ ] App launches without exception
[ ] Custom title bar visible (no default Windows chrome)
[ ] Window can be dragged by title bar
[ ] Minimize, maximize, close buttons work
[ ] Window position and size are remembered between launches
[ ] Navigation works between all pages
[ ] Each page shows its ViewModel name (helpful during development)
[ ] Status bar visible with app version
```

---

## Gate 4: Per-Feature Gates

Run these for EVERY feature before moving to the next:

```
[ ] Feature works on golden path (normal usage, valid input)
[ ] Feature handles empty/null state (no data, nothing selected)
[ ] Feature handles error state (network down, bad input, permission denied)
[ ] Error state is visible to user (not just in logs)
[ ] All state changes are logged at appropriate level
[ ] Loading states show a progress indicator (not a frozen UI)
[ ] Feature works after navigating away and returning
[ ] Feature works after app minimize/restore
```

---

## Gate 5: Visual Quality

```
[ ] No hardcoded colors — all from ResourceDictionary
[ ] No hardcoded pixel sizes — all from ResourceDictionary spacing scale
[ ] All text readable at 150% DPI scaling
[ ] All interactive elements have hover states
[ ] All interactive elements have pressed/active states
[ ] All interactive elements have disabled states
[ ] Layout doesn't break at 800x600 (minimum window size)
[ ] Layout doesn't break at 3840x2160 (4K)
[ ] No elements overlap or get clipped at any tested resolution
[ ] Animations run at 60 FPS (check DiagnosticsOverlay during animation)
```

---

## Gate 6: Data & Persistence

```
[ ] User settings are saved between sessions
[ ] App does not crash on corrupted settings file (handles gracefully)
[ ] No sensitive data written to log files
[ ] File operations handle permissions errors gracefully
[ ] Long-running operations are cancellable
```

---

## Gate 7: Final Verification

```
[ ] Uninstall all dev dependencies, run on clean machine (or VM)
  -OR-
  Run from a path with no Visual Studio present
[ ] App icon is custom (not default WPF icon)
[ ] About page shows correct version number
[ ] Splash screen shows correctly on first load
[ ] All keyboard shortcuts work (Ctrl+Shift+D, L, S, R)
[ ] No console window visible in release build
[ ] Memory usage is stable after 30 minutes of use (no leak)
```

---

## Quick Reference: Common Failures

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| App freezes during async operation | Async code not properly awaited on UI thread | Use `Dispatcher.InvokeAsync` or `Task.Run` properly |
| Binding shows nothing | DataContext not set, or binding path typo | Check Output window for binding errors |
| Animation stutters | Heavy operation on UI thread during animation | Move work to background thread |
| Memory grows over time | Event subscriptions not unsubscribed | Implement IDisposable, unsubscribe in Dispose |
| Crash on second launch | Settings file corrupt from first crash | Add try/catch around settings load |
| Controls overlap at small window | No minimum sizes set | Add MinWidth/MinHeight to Grid columns/rows |
