using Serilog;

namespace BaseApp.Core.Services;

public class NavigationService : INavigationService
{
    private readonly Stack<string> _history = new();

    public string CurrentView { get; private set; } = string.Empty;

    public event EventHandler<string>? Navigated;

    public void NavigateTo(string viewName)
    {
        if (!string.IsNullOrWhiteSpace(CurrentView))
            _history.Push(CurrentView);

        CurrentView = viewName;
        Log.Information("[Navigation] → {ViewName} (history depth: {Depth})", viewName, _history.Count);
        Navigated?.Invoke(this, viewName);
    }

    public void GoBack()
    {
        if (_history.Count == 0)
        {
            Log.Debug("[Navigation] GoBack called but history is empty");
            return;
        }

        var previous = _history.Pop();
        CurrentView = previous;
        Log.Information("[Navigation] ← {ViewName} (back)", previous);
        Navigated?.Invoke(this, previous);
    }
}
