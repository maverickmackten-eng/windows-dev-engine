using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Serilog;

namespace __APP_NAME__
{
    /// <summary>
    /// Splash screen window.
    ///
    /// USAGE in App.xaml.cs OnStartup():
    ///   var splash = new SplashWindow();
    ///   splash.Show();
    ///   await splash.RunInitSequenceAsync(() => YourInitMethod());
    ///   // splash closes itself; then show MainWindow
    ///
    /// CUSTOMISE:
    ///   - Replace __APP_DISPLAY_NAME__ and __APP_TAGLINE__ in XAML
    ///   - Call UpdateStatus(message, percent) from your init steps
    /// </summary>
    public partial class SplashWindow : Window
    {
        private const int MinDisplayMs = 2000; // Never flash away faster than 2s

        public SplashWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Runs your initialization delegate while animating the progress bar.
        /// Ensures the splash is visible for at least MinDisplayMs milliseconds.
        /// Fades out and closes when done.
        /// </summary>
        public async Task RunInitSequenceAsync(Func<IProgress<(string message, int percent)>, Task> initAction)
        {
            var started = DateTime.UtcNow;
            Log.Debug("[SplashWindow] Init sequence started");

            var progress = new Progress<(string message, int percent)>(report =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtStatus.Text = report.message;
                    AnimateProgressTo(report.percent);
                });
            });

            try
            {
                await initAction(progress);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[SplashWindow] Init sequence failed");
                throw;
            }

            // Ensure minimum display time
            var elapsed = (DateTime.UtcNow - started).TotalMilliseconds;
            if (elapsed < MinDisplayMs)
                await Task.Delay((int)(MinDisplayMs - elapsed));

            // Fill to 100% then fade out
            AnimateProgressTo(100);
            await Task.Delay(300);
            await FadeOutAsync();

            Log.Debug("[SplashWindow] Splash dismissed after {Ms}ms",
                (DateTime.UtcNow - started).TotalMilliseconds);
            Close();
        }

        private void AnimateProgressTo(double target)
        {
            var anim = new DoubleAnimation(target, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            splashProgress.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, anim);
        }

        private Task FadeOutAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400));
            anim.Completed += (_, _) => tcs.SetResult(true);
            BeginAnimation(OpacityProperty, anim);
            return tcs.Task;
        }
    }
}
