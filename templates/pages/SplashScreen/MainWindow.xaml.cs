using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SplashScreen;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Simulate loading stages — replace with real initialization work
        await UpdateStatus("Loading configuration...", 600);
        await UpdateStatus("Connecting to services...", 700);
        await UpdateStatus("Initializing debug infrastructure...", 500);
        await UpdateStatus("Loading assets...", 600);
        await UpdateStatus("Ready.", 400);

        await Task.Delay(500);

        // Launch main window and close splash
        // var main = new MainApplication.MainWindow();
        // main.Show();
        Close();
    }

    private async Task UpdateStatus(string message, int delayMs)
    {
        statusText.Text = message;
        await Task.Delay(delayMs);
    }
}
