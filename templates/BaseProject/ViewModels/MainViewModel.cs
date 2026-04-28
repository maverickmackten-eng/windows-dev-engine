using BaseApp.Core.Services;
using BaseApp.ViewModels.Base;
using Serilog;

namespace BaseApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private string _currentPage = "Dashboard";
    private string _statusMessage = "Ready";

    public MainViewModel(INavigationService nav)
    {
        _nav = nav;
        _nav.Navigated += OnNavigated;
        Log.Debug("[MainViewModel] Initialized");
    }

    public string CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private void OnNavigated(object? sender, string viewName)
    {
        CurrentPage = viewName;
        Log.Debug("[MainViewModel] Page updated to {ViewName}", viewName);
    }
}
