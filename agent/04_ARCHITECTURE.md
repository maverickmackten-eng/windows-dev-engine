# 04 — Architecture

## Project Structure

Every application built with this engine uses this exact folder structure.
Do not deviate. Consistency lets any agent pick up any project immediately.

```
YourAppName/
├── YourAppName.sln
├── YourAppName/
│   ├── YourAppName.csproj
│   ├── App.xaml
│   ├── App.xaml.cs                  ← Startup, logging, DI container
│   │
│   ├── Assets/
│   │   ├── Themes/
│   │   │   ├── Colors.xaml          ← Color ResourceDictionary
│   │   │   ├── Typography.xaml      ← Font ResourceDictionary
│   │   │   ├── Controls.xaml        ← Control style overrides
│   │   │   └── Animations.xaml      ← Shared storyboard resources
│   │   └── Icons/                   ← SVG or XAML path-based icons only
│   │
│   ├── Core/
│   │   ├── Services/
│   │   │   ├── INavigationService.cs
│   │   │   └── NavigationService.cs
│   │   ├── Commands/
│   │   │   └── RelayCommand.cs      ← ICommand implementation
│   │   └── Extensions/
│   │       └── ObservableCollectionExtensions.cs
│   │
│   ├── Models/                      ← Pure data, no UI references
│   │   └── FeatureName.cs
│   │
│   ├── ViewModels/                  ← Business logic, state, commands
│   │   ├── Base/
│   │   │   └── ViewModelBase.cs     ← INotifyPropertyChanged base
│   │   ├── MainViewModel.cs
│   │   └── FeatureViewModel.cs
│   │
│   ├── Views/                       ← XAML only, minimal code-behind
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── Pages/
│   │   │   ├── DashboardPage.xaml
│   │   │   └── SettingsPage.xaml
│   │   └── Controls/
│   │       └── StatusBar.xaml       ← Reusable UserControls
│   │
│   ├── Debug/                       ← Debug infrastructure (always present)
│   │   ├── DiagnosticsOverlay.xaml
│   │   ├── DiagnosticsOverlay.xaml.cs
│   │   └── DiagnosticsViewModel.cs
│   │
│   └── Properties/
│       └── launchSettings.json
│
└── YourAppName.Tests/               ← xUnit test project
    └── ViewModels/
        └── FeatureViewModelTests.cs
```

---

## MVVM Rules

### Model
- Plain C# classes
- No UI references (no WPF namespaces)
- Can use INotifyPropertyChanged if needed for reactive models
- Serializable if used for persistence

### ViewModel
- Inherits `ViewModelBase` (INotifyPropertyChanged)
- All state as public properties with `SetProperty()`
- All user actions as `ICommand` (RelayCommand)
- No direct View references — use events or services
- Constructor receives services via dependency injection

```csharp
public class DashboardViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private string _statusMessage = "Ready";

    public DashboardViewModel(INavigationService nav)
    {
        _nav = nav;
        NavigateCommand = new RelayCommand<string>(Navigate);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand NavigateCommand { get; }

    private void Navigate(string target)
    {
        Log.Information("Navigating to {Target}", target);
        _nav.NavigateTo(target);
    }
}
```

### View
- XAML declares UI structure
- Code-behind: ONLY event→command wiring, animation triggers
- DataContext set by navigation service or App.xaml.cs
- No business logic. No data fetching. No direct model access.

```xml
<UserControl x:Class="YourApp.Views.Pages.DashboardPage"
             DataContext="{Binding Source={StaticResource Locator}, Path=Dashboard}">
    <Grid>
        <TextBlock Text="{Binding StatusMessage}" />
        <Button Content="Go to Settings"
                Command="{Binding NavigateCommand}"
                CommandParameter="Settings" />
    </Grid>
</UserControl>
```

---

## ViewModelBase

```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

---

## RelayCommand

```csharp
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        parameter is T t && (_canExecute?.Invoke(t) ?? true);

    public void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            Log.Debug("[Command] {CommandType} executing with {Parameter}",
                GetType().Name, parameter);
            _execute(t);
        }
    }
}
```

---

## Navigation Service

```csharp
public interface INavigationService
{
    void NavigateTo(string viewName);
    void GoBack();
    string CurrentView { get; }
    event EventHandler<string> Navigated;
}
```

Navigation maps string names to View types. The MainWindow hosts a `Frame` or
`ContentControl` that swaps content. ViewModels are resolved from DI container.

---

## Dependency Injection

Use `Microsoft.Extensions.DependencyInjection` (built into .NET):

```csharp
// In App.xaml.cs
private IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Services
    services.AddSingleton<INavigationService, NavigationService>();

    // ViewModels
    services.AddTransient<MainViewModel>();
    services.AddTransient<DashboardViewModel>();

    return services.BuildServiceProvider();
}
```

---

## Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Classes | PascalCase | `DashboardViewModel` |
| Interfaces | IPascalCase | `INavigationService` |
| Private fields | _camelCase | `_statusMessage` |
| Properties | PascalCase | `StatusMessage` |
| Commands | PascalCase + Command | `SaveCommand` |
| XAML x:Name | camelCase | `statusTextBlock` |
| Pages | PascalCase + Page | `DashboardPage` |
| UserControls | PascalCase + Control | `StatusBarControl` |

---

## .csproj Template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>YourAppName</AssemblyName>
    <RootNamespace>YourAppName</RootNamespace>
    <Version>1.0.0</Version>
    <ApplicationIcon>Assets\app.ico</ApplicationIcon>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.*" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.*" />
    <PackageReference Include="Serilog.Sinks.Debug" Version="3.*" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="3.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.*" />
  </ItemGroup>
</Project>
```
