using System.Windows;
using System.Windows.Input;
using BaseApp.ViewModels;
using Serilog;

namespace BaseApp.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        SetupKeyboardShortcuts();
        Log.Information("[MainWindow] Initialized");
    }

    private void SetupKeyboardShortcuts()
    {
        KeyDown += (s, e) =>
        {
            if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Key.D:
                        ToggleDiagnosticsOverlay();
                        e.Handled = true;
                        break;
                    case Key.L:
                        OpenLogFile();
                        e.Handled = true;
                        break;
                    case Key.S:
                        DumpState();
                        e.Handled = true;
                        break;
                    case Key.R:
                        RestartApp();
                        e.Handled = true;
                        break;
                }
            }
        };
    }

    private void ToggleDiagnosticsOverlay()
    {
        diagnosticsOverlay.Visibility = diagnosticsOverlay.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        Log.Debug("[MainWindow] DiagnosticsOverlay toggled: {State}", diagnosticsOverlay.Visibility);
    }

    private static void OpenLogFile()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BaseApp", "logs");
        var latestLog = Directory.GetFiles(logDir, "*.log").OrderByDescending(f => f).FirstOrDefault();
        if (latestLog != null)
            System.Diagnostics.Process.Start("explorer.exe", latestLog);
        else
            Log.Warning("[MainWindow] No log file found in {Dir}", logDir);
    }

    private void DumpState()
    {
        Log.Information("[StateSnapshot] CurrentPage={Page} StatusMessage={Status}",
            _viewModel.CurrentPage,
            _viewModel.StatusMessage);
    }

    private static void RestartApp()
    {
        Log.Information("[MainWindow] Manual restart triggered");
        System.Diagnostics.Process.Start(
            Environment.ProcessPath ?? "BaseApp.exe");
        Application.Current.Shutdown();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Log.Information("[MainWindow] User closed window");
        Close();
    }
}
