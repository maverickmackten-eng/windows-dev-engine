namespace BaseApp.Core.Services;

public interface INavigationService
{
    string CurrentView { get; }
    void NavigateTo(string viewName);
    void GoBack();
    event EventHandler<string> Navigated;
}
