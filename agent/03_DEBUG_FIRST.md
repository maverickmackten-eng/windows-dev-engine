# 03 — Debug First

## The Law

> **Debugging is more important than functionality.**

A feature that works but cannot be debugged is a liability.
A debug-wired skeleton with no features is a foundation.

Always wire debug infrastructure FIRST. No exceptions. No "I'll add it later."

---

## Why This Matters

When an app breaks in production (and it will):
- Without debug infrastructure: you are blind. You guess. You waste hours.
- With debug infrastructure: you open the log, see the exact state at failure, fix it in minutes.

Commander Maverick builds tools to solve problems. A tool that breaks without explanation
is worse than no tool at all — it destroys trust in the application.

---

## The Debug Stack

Every application built with this engine uses:

| Component | Purpose |
|-----------|---------|
| **Serilog** | Structured logging to file + debug console |
| **DiagnosticsOverlay** | Always-visible FPS, memory, active view, last log line |
| **GlobalExceptionHandler** | Catches every unhandled exception, logs it, shows user message |
| **CommandLogger** | Logs every ICommand execution with timing |
| **NavigationLogger** | Logs every page navigation |
| **StateSnapshot** | On demand: dumps all ViewModel state to log |

---

## Serilog Setup (Wire In App.xaml.cs)

```csharp
private void ConfigureLogging()
{
    string logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YourAppName", "logs", "app-.log");

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message}{NewLine}{Exception}")
        .WriteTo.File(
            logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    Log.Information("=== {AppName} v{Version} starting ===",
        AppDomain.CurrentDomain.FriendlyName,
        Assembly.GetExecutingAssembly().GetName().Version);
}
```

---

## Global Exception Handler (Wire In App.xaml.cs)

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    ConfigureLogging();

    // Catch all unhandled exceptions
    DispatcherUnhandledException += (s, ex) =>
    {
        Log.Fatal(ex.Exception, "Unhandled UI thread exception");
        MessageBox.Show(
            $"An unexpected error occurred. Details saved to log.\n\n{ex.Exception.Message}",
            "Application Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        ex.Handled = true; // Prevent crash — keep app alive
    };

    AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
    {
        Log.Fatal(ex.ExceptionObject as Exception, "Unhandled non-UI thread exception");
    };

    TaskScheduler.UnobservedTaskException += (s, ex) =>
    {
        Log.Error(ex.Exception, "Unobserved task exception");
        ex.SetObserved();
    };

    base.OnStartup(e);
}
```

---

## DiagnosticsOverlay

The diagnostics overlay is a semi-transparent HUD always available via Ctrl+Shift+D.
It shows:

```
┌─────────────────────────────────────┐
│ [DEBUG MODE]                        │
│ FPS: 60    Memory: 128 MB           │
│ View: DashboardView                 │
│ VM:   DashboardViewModel            │
│ Last Log: [14:23:01] Nav → Dashboard│
│ Uptime: 00:12:34                    │
└─────────────────────────────────────┘
```

See `debug/DiagnosticsOverlay/` for the complete implementation.

---

## Logging Conventions

### Log Levels

| Level | When To Use |
|-------|-------------|
| `Log.Verbose` | Very noisy inner-loop events (disabled in release) |
| `Log.Debug` | State changes, normal flow events |
| `Log.Information` | Major milestones: startup, navigation, user actions |
| `Log.Warning` | Unexpected but recoverable situations |
| `Log.Error` | Caught exceptions, feature failures |
| `Log.Fatal` | Unrecoverable failures, app about to exit |

### What To Log

```csharp
// Navigation
Log.Information("Navigating to {ViewName}", nameof(DashboardView));

// User actions
Log.Debug("User clicked {ButtonName}, current state: {State}", "SaveButton", _state);

// Data operations
Log.Debug("Loading {ItemCount} records from {Source}", items.Count, sourceName);

// Errors — ALWAYS include exception object and context
Log.Error(ex, "Failed to save {FileName} after {RetryCount} attempts", fileName, retries);

// Performance
var sw = Stopwatch.StartNew();
// ... operation ...
Log.Debug("Operation completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

### What NOT To Log

- Passwords, tokens, or any sensitive data
- Full file contents (log the path and size instead)
- High-frequency events in a render loop without throttling

---

## Debug Keyboard Shortcuts

Every app must implement these:

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+D` | Toggle DiagnosticsOverlay |
| `Ctrl+Shift+L` | Open log file in default viewer |
| `Ctrl+Shift+S` | Dump current ViewModel state to log |
| `Ctrl+Shift+R` | Restart app (for testing startup sequence) |

---

## ViewModel Debug Pattern

Every ViewModel should call this when a critical state change happens:

```csharp
private void OnStateChanged(string context)
{
    Log.Debug("[{ViewModel}] {Context} — State: {@State}", 
        GetType().Name, 
        context, 
        new { Property1, Property2, CurrentStatus });
}
```

---

## The Debug-First Checklist (Before ANY Feature Work)

```
[ ] Serilog installed and configured (3 sinks: file, debug console)
[ ] Log file appears in LocalAppData after first run
[ ] GlobalExceptionHandler wired — tested by throwing an exception manually
[ ] DiagnosticsOverlay visible via Ctrl+Shift+D
[ ] FPS counter updating in DiagnosticsOverlay
[ ] Memory usage visible in DiagnosticsOverlay
[ ] Navigation events appearing in DiagnosticsOverlay
[ ] Ctrl+Shift+L opens log file
```

Only after all boxes are checked: write your first feature.
