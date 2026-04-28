# Getting Started with Windows Dev Engine

This guide walks you from zero to a running WPF app in 5 steps using the Windows Dev Engine scaffold.

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 8.0 or later |
| Windows | 10 1903+ / 11 |
| IDE | Visual Studio 2022 17.8+ **or** VS Code + C# Dev Kit |
| Git | Any recent version |

---

## Step 1 — Clone the Repository

```bash
git clone https://github.com/maverickmackten-eng/windows-dev-engine.git
cd windows-dev-engine
```

Open the solution:

```bash
start WindowsDevEngine.sln
```

Or in VS Code:

```bash
code .
```

---

## Step 2 — Create Your App Project

Create a new WPF project inside the repo:

```bash
dotnet new wpf -n MyApp -f net8.0-windows -o apps/MyApp
```

Add it to the solution:

```bash
dotnet sln WindowsDevEngine.sln add apps/MyApp/MyApp.csproj
```

Add the Serilog packages used throughout the engine:

```bash
cd apps/MyApp
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Debug
dotnet add package Microsoft.Extensions.DependencyInjection
```

---

## Step 3 — Wire Up the App Entry Point

Replace the default `App.xaml.cs` with the engine bootstrap pattern:

```csharp
// App.xaml.cs
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MyApp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 1. Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("logs/myapp-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            Log.Information("[App] Starting up");

            // 2. Register services
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            // 3. Show splash while initializing
            var splash = new Views.SplashWindow();
            splash.Show();

            await splash.RunInitializationAsync(async status =>
            {
                status("Loading configuration...");
                await Services.GetRequiredService<Services.SettingsService>().LoadAsync();

                status("Initializing data...");
                // await any other async init here
            });

            // 4. Show main window
            var main = Services.GetRequiredService<MainWindow>();
            main.Show();

            base.OnStartup(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // ViewModels
            services.AddTransient<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.DashboardViewModel>();
            services.AddTransient<ViewModels.SettingsViewModel>();

            // Services
            services.AddSingleton<Services.SettingsService>();

            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("[App] Exiting");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
```

Update `App.xaml` — remove `StartupUri` (we launch from `OnStartup`):

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <!-- Merge your chosen theme here -->
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/DarkMilitary.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

---

## Step 4 — Copy Templates into Your App

Pick the page and control templates you need from `/templates/` and copy them in:

```
templates/
  pages/
    SplashScreen/     → copy to apps/MyApp/Views/
    DashboardPage/    → copy to apps/MyApp/Views/Pages/
    SettingsPage/     → copy to apps/MyApp/Views/Pages/
    EmptyPage/        → copy to apps/MyApp/Views/Pages/  (use as scaffold)
  controls/
    ToastNotification/  → copy to apps/MyApp/Views/Controls/
    LoadingOverlay/     → copy to apps/MyApp/Views/Controls/
    ConfirmDialog/      → copy to apps/MyApp/Views/Controls/
```

Search-and-replace `__APP_NAME__` with `MyApp` in all copied files:

```powershell
# PowerShell one-liner
Get-ChildItem -Recurse -Filter *.cs,*.xaml apps/MyApp |
  ForEach-Object { (Get-Content $_.FullName) -replace '__APP_NAME__','MyApp' |
    Set-Content $_.FullName }
```

---

## Step 5 — Run and Verify

```bash
cd apps/MyApp
dotnet run
```

Expected startup sequence:
1. Splash window appears, progress bar animates
2. `SettingsService.LoadAsync()` reads (or creates) `settings.json`
3. Splash fades out, `MainWindow` appears
4. Serilog writes to `logs/myapp-YYYYMMDD.log`

---

## Project Layout Convention

```
MyApp/
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs
  Views/
    SplashWindow.xaml(.cs)
    Pages/
      DashboardPage.xaml(.cs)
      SettingsPage.xaml(.cs)
    Controls/
      ToastNotification.xaml(.cs)
      LoadingOverlay.xaml(.cs)
      ConfirmDialog.xaml(.cs)
  ViewModels/
    ViewModelBase.cs
    MainViewModel.cs
    DashboardViewModel.cs
    SettingsViewModel.cs
  Services/
    SettingsService.cs
  Themes/
    DarkMilitary.xaml
  Models/
    AppSettings.cs
  logs/         (runtime, git-ignored)
  settings.json (runtime, git-ignored)
```

---

## Next Steps

- **Theming** → see [`THEMING.md`](THEMING.md)
- **Controls reference** → see [`CONTROLS.md`](CONTROLS.md)
- **MVVM + DI architecture** → see [`ARCHITECTURE.md`](ARCHITECTURE.md)
- **Live samples** → browse `samples/` and run any `.csproj` directly
