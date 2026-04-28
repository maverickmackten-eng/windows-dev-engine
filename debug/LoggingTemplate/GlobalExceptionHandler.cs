using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace __APP_NAME__.Debug
{
    /// <summary>
    /// Drop-in global exception handler. Wire all three handlers in
    /// App.xaml.cs OnStartup AFTER calling LoggingSetup.ConfigureLogging().
    ///
    /// USAGE:
    ///   1. Find+Replace __APP_NAME__ with your actual namespace
    ///   2. Call GlobalExceptionHandler.Wire(this) in OnStartup()
    ///
    /// TESTING:
    ///   Add this temporarily to a button click to verify wiring:
    ///     throw new InvalidOperationException("Test exception — remove me");
    /// </summary>
    public static class GlobalExceptionHandler
    {
        private static Application? _app;

        public static void Wire(Application app)
        {
            _app = app;

            // 1. UI thread exceptions (most common — binding errors, command crashes)
            app.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // 2. Non-UI thread exceptions (background workers, Task.Run)
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            // 3. Async/await exceptions that were never observed
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Log.Debug("[GlobalExceptionHandler] Wired — all exception channels active");
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception,
                "[GlobalExceptionHandler] Unhandled UI thread exception: {Message}",
                e.Exception.Message);

            ShowErrorDialog(e.Exception);

            e.Handled = true; // Keep app alive — log shows what happened
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log.Fatal(ex,
                "[GlobalExceptionHandler] Unhandled non-UI exception (IsTerminating={IsTerminating}): {Message}",
                e.IsTerminating,
                ex?.Message ?? "Unknown");

            // Cannot prevent termination here — ensure log is flushed
            Log.CloseAndFlush();
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception,
                "[GlobalExceptionHandler] Unobserved Task exception: {Message}",
                e.Exception.Message);

            e.SetObserved(); // Prevent process crash for fire-and-forget tasks
        }

        private static void ShowErrorDialog(Exception ex)
        {
            // Run on UI thread (this IS the UI thread for Dispatcher exceptions,
            // but guard in case called from elsewhere)
            _app?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred. The application will continue running.\n\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"Full details saved to log file (Ctrl+Shift+L to open).",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
    }
}
