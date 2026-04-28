using System;
using System.IO;
using System.Reflection;
using Serilog;

namespace __APP_NAME__.Debug
{
    /// <summary>
    /// Drop-in logging setup. Call ConfigureLogging() as the FIRST line of
    /// App.xaml.cs OnStartup — before anything else touches the app.
    ///
    /// USAGE:
    ///   1. Find+Replace __APP_NAME__ with your actual namespace
    ///   2. Call LoggingSetup.ConfigureLogging() at the top of OnStartup()
    ///   3. Call Log.CloseAndFlush() in App.xaml.cs OnExit()
    /// </summary>
    public static class LoggingSetup
    {
        public static void ConfigureLogging()
        {
            string appName = AppDomain.CurrentDomain.FriendlyName;
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "logs");

            Directory.CreateDirectory(logDirectory);

            string logPath = Path.Combine(logDirectory, "app-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                // Sink 1: Visual Studio Output window (real-time during dev)
                .WriteTo.Debug(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message}{NewLine}{Exception}")
                // Sink 2: Rolling daily log file (7-day retention)
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                // Sink 3: Seq (uncomment when running a local Seq instance)
                // .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("=== {AppName} v{Version} starting ===", appName, version);
            Log.Information("Log path: {LogPath}", logPath);
        }

        /// <summary>
        /// Call this from App.xaml.cs OnExit to flush any buffered log entries.
        /// </summary>
        public static void Shutdown()
        {
            Log.Information("=== Application shutting down ===");
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Opens the log folder in Windows Explorer. Bind to Ctrl+Shift+L.
        /// </summary>
        public static void OpenLogFolder()
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppDomain.CurrentDomain.FriendlyName,
                "logs");

            if (Directory.Exists(logDirectory))
                System.Diagnostics.Process.Start("explorer.exe", logDirectory);
        }
    }
}
