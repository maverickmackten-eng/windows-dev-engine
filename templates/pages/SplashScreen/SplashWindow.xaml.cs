using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Serilog;

namespace __APP_NAME__
{
    /// <summary>
    /// Splash screen shown during app initialization.
    /// USAGE in App.xaml.cs OnStartup:
    ///   var splash = new SplashWindow();
    ///   splash.Show();
    ///   await splash.RunInitializationAsync(async status =>
    ///   {
    ///       status("Loading configuration...");
    ///       await LoadConfigAsync();
    ///   });
    ///   var main = new MainWindow();
    ///   main.Show();
    /// </summary>
    public partial class SplashWindow : Window
    {
        private TaskCompletionSource<bool>? _fadeOutComplete;

        public SplashWindow() { InitializeComponent(); Log.Debug("[SplashWindow] Displayed"); }

        public async Task RunInitializationAsync(Func<Action<string>, Task> initWork)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ((Storyboard)FindResource("ProgressStoryboard")).Begin();
            try
            {
                await initWork(msg => Dispatcher.Invoke(() =>
                {
                    StatusText.Text = msg;
                    Log.Debug("[Splash] {Status}", msg);
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[SplashWindow] Init exception");
                Dispatcher.Invoke(() => StatusText.Text = "Startup error — check logs");
            }
            var remaining = 2000 - (int)sw.ElapsedMilliseconds;
            if (remaining > 0) await Task.Delay(remaining);
            _fadeOutComplete = new TaskCompletionSource<bool>();
            Dispatcher.Invoke(() => ((Storyboard)FindResource("FadeOutStoryboard")).Begin());
            await _fadeOutComplete.Task;
        }

        private void FadeOut_Completed(object? sender, EventArgs e)
        {
            Log.Debug("[SplashWindow] Fade-out complete");
            Close();
            _fadeOutComplete?.SetResult(true);
        }
    }
}
