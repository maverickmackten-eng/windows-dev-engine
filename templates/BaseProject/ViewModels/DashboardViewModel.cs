using BaseApp.Core.Commands;
using BaseApp.Core.Services;
using BaseApp.ViewModels.Base;
using Serilog;

namespace BaseApp.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private string _statusMessage = "All systems nominal";
    private bool _isLoading;

    public DashboardViewModel(INavigationService nav)
    {
        _nav = nav;
        RefreshCommand = new RelayCommand(OnRefresh);
        NavigateCommand = new RelayCommand<string>(OnNavigate);
        Log.Debug("[DashboardViewModel] Initialized");
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand<string> NavigateCommand { get; }

    private async void OnRefresh()
    {
        Log.Information("[DashboardViewModel] Refresh triggered");
        IsLoading = true;
        StatusMessage = "Refreshing...";

        try
        {
            await Task.Delay(1500); // Replace with real async work
            StatusMessage = "All systems nominal";
            Log.Information("[DashboardViewModel] Refresh complete");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[DashboardViewModel] Refresh failed");
            StatusMessage = "Refresh failed — check log";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnNavigate(string target)
    {
        _nav.NavigateTo(target);
    }
}
