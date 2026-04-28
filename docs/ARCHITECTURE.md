# Architecture Guide

Windows Dev Engine apps follow a strict MVVM pattern with constructor-injected dependencies.
This document covers the full wiring: `ViewModelBase`, DI registration, navigation, settings,
logging, and error handling.

---

## Overview

```
┌──────────────────────────────────────────────────────────┐
│  View (XAML)                                              │
│    └─ DataContext = ViewModel (set by DI / code-behind)   │
└──────────────────────────┬──────────────────────────────┘
                         │  Bindings / Commands
                         ▼
┌──────────────────────────────────────────────────────────┐
│  ViewModel (: ViewModelBase)                              │
│    ├─ ObservableProperties (SetField + OnPropertyChanged) │
│    ├─ RelayCommands (ICommand)                            │
│    └─ Injects: ISettingsService, ILogger, etc.            │
└──────────────────────────┬──────────────────────────────┘
                         │  Method calls
                         ▼
┌──────────────────────────────────────────────────────────┐
│  Services (Singleton / Transient)                        │
│    ├─ SettingsService   (JSON persist / load)             │
│    ├─ NavigationService (page routing)                   │
│    └─ [Your domain services here]                         │
└──────────────────────────────────────────────────────────┘
```

**Rules:**
- Views never talk to Services directly
- ViewModels never reference `System.Windows` types (keeps them testable)
- Services are interface-based so they can be mocked in unit tests

---

## ViewModelBase

All ViewModels inherit from `ViewModelBase`. Copy this into `ViewModels/ViewModelBase.cs`:

```csharp
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;

namespace MyApp.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // ── Property change helper ─────────────────────────────────
        protected bool SetField<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ── Loading / error state (shared UI pattern) ───────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        // ── Guarded async execution ──────────────────────────────
        /// <summary>
        /// Wraps an async operation: sets IsLoading, clears ErrorMessage,
        /// catches exceptions, logs them, and sets ErrorMessage on failure.
        /// </summary>
        protected async Task RunAsync(System.Func<Task> work,
            [CallerMemberName] string? caller = null)
        {
            IsLoading    = true;
            ErrorMessage = null;
            try
            {
                await work();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
                Log.Error(ex, "[{VM}.{Caller}] Unhandled exception",
                    GetType().Name, caller);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // ───────────────────────────────────────────────────────────────
    // RelayCommand — ICommand implementation for MVVM
    // ───────────────────────────────────────────────────────────────
    public class RelayCommand : ICommand
    {
        private readonly System.Action<object?> _execute;
        private readonly System.Func<object?, bool>? _canExecute;

        public RelayCommand(System.Action<object?> execute,
                            System.Func<object?, bool>? canExecute = null)
        {
            _execute    = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(System.Action execute,
                            System.Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute is null ? null : _ => canExecute()) { }

        public event System.EventHandler? CanExecuteChanged
        {
            add    => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? p)  => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p)     => _execute(p);
        public void RaiseCanExecuteChanged()
            => System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    // Async variant
    public class AsyncRelayCommand : ICommand
    {
        private readonly System.Func<Task> _execute;
        private readonly System.Func<bool>? _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(System.Func<Task> execute,
                                 System.Func<bool>? canExecute = null)
        {
            _execute    = execute;
            _canExecute = canExecute;
        }

        public event System.EventHandler? CanExecuteChanged
        {
            add    => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? _) => !_isRunning && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? _)
        {
            _isRunning = true;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            try   { await _execute(); }
            finally
            {
                _isRunning = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
```

---

## SettingsService

Typed JSON settings with atomic save. Copy to `Services/SettingsService.cs`:

```csharp
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace MyApp.Services
{
    public class SettingsService
    {
        private static readonly string _path =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented  = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public Models.AppSettings Current { get; private set; } = new();

        public async Task LoadAsync()
        {
            try
            {
                if (!File.Exists(_path)) { await SaveAsync(); return; }
                var json = await File.ReadAllTextAsync(_path);
                Current = JsonSerializer.Deserialize<Models.AppSettings>(json, _opts)
                          ?? new Models.AppSettings();
                Log.Debug("[Settings] Loaded from {Path}", _path);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[Settings] Failed to load, using defaults");
                Current = new Models.AppSettings();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, _opts);
                // Atomic write via temp file
                var tmp = _path + ".tmp";
                await File.WriteAllTextAsync(tmp, json);
                File.Move(tmp, _path, overwrite: true);
                Log.Debug("[Settings] Saved");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Settings] Save failed");
            }
        }
    }
}
```

And the model `Models/AppSettings.cs`:

```csharp
namespace MyApp.Models
{
    public class AppSettings
    {
        public bool   EnableAnimations  { get; set; } = true;
        public bool   StartMinimized    { get; set; } = false;
        public int    AutoSaveInterval  { get; set; } = 5;     // minutes
        public string DefaultOutputPath { get; set; } = "";
        public string Theme             { get; set; } = "DarkMilitary";
        public string AppVersion        { get; set; } = "1.0.0";
    }
}
```

---

## NavigationService

A lightweight in-process page router. Copy to `Services/NavigationService.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Windows;
using Serilog;

namespace MyApp.Services
{
    public class NavigationService
    {
        // Raised when the current page changes.
        // Args: (pageName: string, pageContent: UIElement)
        public event Action<string, UIElement>? Navigated;

        private readonly IServiceProvider _sp;
        private readonly Dictionary<string, Func<UIElement>> _routes = new();

        public string CurrentPage { get; private set; } = "";

        public NavigationService(IServiceProvider sp) { _sp = sp; }

        /// <summary>Register a named route to a factory function.</summary>
        public void Register(string name, Func<UIElement> factory)
            => _routes[name] = factory;

        /// <summary>Navigate to a named route. No-op if already on that page.</summary>
        public void NavigateTo(string name)
        {
            if (CurrentPage == name) return;
            if (!_routes.TryGetValue(name, out var factory))
            {
                Log.Warning("[Nav] Unknown route: {Route}", name);
                return;
            }
            CurrentPage = name;
            var page = factory();
            Navigated?.Invoke(name, page);
            Log.Debug("[Nav] -> {Page}", name);
        }
    }
}
```

Wire it in `MainWindow.xaml.cs`:

```csharp
protected override void OnContentRendered(EventArgs e)
{
    base.OnContentRendered(e);
    _nav.Register("Dashboard", () => new Views.Pages.DashboardPage { DataContext = App.Services.GetRequiredService<ViewModels.DashboardViewModel>() });
    _nav.Register("Settings",  () => new Views.Pages.SettingsPage  { DataContext = App.Services.GetRequiredService<ViewModels.SettingsViewModel>() });
    _nav.Navigated += (name, page) => ContentHost.Content = page;
    _nav.NavigateTo("Dashboard");
}
```

---

## DI Registration Cheat Sheet

| Lifetime | Use when |
|---|---|
| `AddSingleton<T>` | State shared app-wide: `SettingsService`, `NavigationService`, cache |
| `AddTransient<T>` | Fresh instance per resolve: ViewModels, dialogs |
| `AddScoped<T>`    | Not commonly used in WPF; use for per-operation units of work |

---

## Global Exception Handling

Add to `App.xaml.cs` `OnStartup` to catch all unhandled exceptions:

```csharp
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    Log.Fatal("[App] UnhandledException: {Ex}", e.ExceptionObject);

Current.DispatcherUnhandledException += (s, e) =>
{
    Log.Fatal(e.Exception, "[App] DispatcherUnhandledException");
    e.Handled = true; // prevent crash
};

System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
{
    Log.Fatal(e.Exception, "[App] UnobservedTaskException");
    e.SetObserved();
};
```

---

## Unit Testing ViewModels

Because ViewModels have no `System.Windows` dependencies, they test without a UI host:

```csharp
[Fact]
public async Task DashboardViewModel_Refresh_SetsIsLoading()
{
    var settings = new SettingsService();
    var vm = new DashboardViewModel(settings);
    Assert.False(vm.IsLoading);
    var task = vm.RefreshCommand.ExecuteAsync();
    Assert.True(vm.IsLoading);
    await task;
    Assert.False(vm.IsLoading);
}
```
