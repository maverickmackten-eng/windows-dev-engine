# Debug Manifesto

## Debugging is Not an Afterthought

In most software projects, debugging is added after features.
In this engine, debugging is the first feature. Always.

This document explains why, and what that means in practice.

---

## The Problem With "Add Logging Later"

When you build a feature without logging, you create a black box.
The feature works — or appears to. You move on.

Three weeks later, the feature breaks. And now you have:
- No log output to tell you what state the app was in
- No diagnostic overlay to tell you what was happening on screen
- No structured exception info to tell you what failed and where
- A user reporting "it just stopped working"

You will spend 3-4x longer debugging that feature than if you had logged it correctly at build time.

Commander Maverick builds tools to solve problems. A tool that breaks opaquely doesn't solve the problem — it creates a new one.

---

## The Debug-First Principle In Practice

```
Traditional order:      Feature → Debug
This engine's order:    Debug → Feature
```

Before the first feature is written, the following must exist and be verified working:

1. **Serilog** writing to file and debug output
2. **GlobalExceptionHandler** catching and logging all exceptions
3. **DiagnosticsOverlay** showing FPS, memory, uptime, current view
4. **Keyboard shortcuts** for toggling overlay and opening log file
5. **CommandLogger** logging every user action

Only when these five items are verified does feature development begin.

---

## What Gets Logged

### Every State Change

```csharp
// WRONG: silent state change
_status = "Loading";

// RIGHT: logged state change
_status = "Loading";
Log.Debug("[{VM}] Status → {Status}", nameof(DashboardViewModel), _status);
```

### Every Command Execution

```csharp
// RelayCommand base handles this automatically:
// Log.Debug("[Command] {Type} executing with {Parameter}", ...)
```

### Every Navigation

```csharp
// NavigationService handles this automatically:
// Log.Information("[Navigation] → {ViewName}", viewName)
```

### Every Error

```csharp
try
{
    await DoSomethingRiskyAsync();
}
catch (Exception ex)
{
    // WRONG: swallow exception
    // return;

    // WRONG: log without context
    // Log.Error("Error occurred");

    // RIGHT: log with full context
    Log.Error(ex, "[{VM}] Failed to {Action} with param {Param}",
        nameof(DashboardViewModel), "LoadData", searchQuery);

    ErrorMessage = "Failed to load data — check log for details";
}
```

### Every Performance-Sensitive Operation

```csharp
var sw = Stopwatch.StartNew();
await HeavyOperation();
sw.Stop();

if (sw.ElapsedMilliseconds > 500)
    Log.Warning("[Perf] {Operation} took {Ms}ms — investigate if this grows", 
        "HeavyOperation", sw.ElapsedMilliseconds);
else
    Log.Debug("[Perf] {Operation} completed in {Ms}ms", "HeavyOperation", sw.ElapsedMilliseconds);
```

---

## The DiagnosticsOverlay Contract

The DiagnosticsOverlay is always present in every app. It is never removed.
In release builds, it is hidden by default but still accessible via Ctrl+Shift+D.

It shows:
- FPS (green if ≥55, yellow if ≥30, red if <30)
- Memory usage in MB
- Uptime
- Current view name
- Last log line

The overlay lets Commander Maverick see app health at a glance without opening a log file.
If something feels wrong — UI lag, memory climbing — a quick Ctrl+Shift+D confirms it.

---

## Log File Conventions

### Location

```
%LOCALAPPDATA%\AppName\logs\app-YYYY-MM-DD.log
```

### Retention

7 days of rolling logs. Older logs are deleted automatically.

### Format

```
2026-04-27 14:23:01.123 [INF] === BaseApp v1.0.0 starting ===
2026-04-27 14:23:01.456 [DBG] [MainViewModel] Initialized
2026-04-27 14:23:02.789 [INF] [Navigation] → Dashboard
2026-04-27 14:23:05.012 [DBG] [DashboardViewModel] Status → Loading
2026-04-27 14:23:06.234 [INF] [DashboardViewModel] Refresh complete
```

### Searching Logs

```powershell
# Find all errors in today's log
Select-String -Path "$env:LOCALAPPDATA\AppName\logs\*.log" -Pattern "\[ERR\]"

# Find navigation events
Select-String -Path "*.log" -Pattern "\[Navigation\]"

# Find slow operations
Select-String -Path "*.log" -Pattern "took \d{3,}ms"
```

---

## When An Exception Escapes To The GlobalExceptionHandler

If an exception reaches the global handler, it means:
1. It was not caught where it should have been
2. It was caught but not logged with context
3. An unhandled async exception was allowed to propagate

When you see GlobalExceptionHandler fire:
1. Check the log for the full stack trace
2. Find the root cause — not where it was caught, where it originated
3. Add a try/catch with context logging AT THE ROOT
4. Add a user-visible error state in the ViewModel
5. Test the failure path again to confirm the error surfaces correctly

---

## Summary

Debug infrastructure is not a luxury. It is the foundation the application sits on.
Build it first. Test it first. Never remove it.

A debuggable application is a maintainable application.
A maintainable application is one that gets better over time, not worse.
