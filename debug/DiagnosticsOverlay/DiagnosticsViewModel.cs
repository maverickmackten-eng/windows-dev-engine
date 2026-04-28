using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Serilog.Core;
using Serilog.Events;

namespace __APP_NAME__.Debug
{
    /// <summary>
    /// ViewModel for DiagnosticsOverlay.xaml.
    /// Drives the real-time HUD: FPS, memory, uptime, current view, last log line.
    ///
    /// SETUP:
    ///   1. Find+Replace __APP_NAME__ with your actual namespace
    ///   2. Set as DataContext on DiagnosticsOverlay in MainWindow
    ///   3. Call SetCurrentView(viewName) from NavigationService on each navigation
    ///   4. Register DiagnosticsSink with Serilog to capture last log line
    /// </summary>
    public sealed class DiagnosticsViewModel : INotifyPropertyChanged, IDisposable
    {
        // ── Timers ──────────────────────────────────────────────────────────────
        private readonly DispatcherTimer _pollTimer;
        private readonly DateTime _startTime = DateTime.Now;

        // ── FPS tracking ────────────────────────────────────────────────────────
        private int _frameCount;
        private DateTime _lastFpsCheck = DateTime.Now;

        // ── Backing fields ──────────────────────────────────────────────────────
        private double _fps;
        private double _memoryMb;
        private string _uptime = "00:00:00";
        private string _currentView = "—";
        private string _currentViewModel = "—";
        private string _lastLogLine = "—";
        private string _fpsColor = "#00FF94";

        // ── Singleton sink reference (set from App.xaml.cs) ─────────────────────
        public static DiagnosticsViewModel? Instance { get; private set; }

        public DiagnosticsViewModel()
        {
            Instance = this;

            // Poll memory + uptime every 500ms
            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _pollTimer.Tick += OnPollTick;
            _pollTimer.Start();

            // Wire FPS counter to the WPF composition engine
            CompositionTarget.Rendering += OnRendering;
        }

        // ── Properties ─────────────────────────────────────────────────────────

        public double Fps
        {
            get => _fps;
            private set => SetProperty(ref _fps, Math.Round(value, 1));
        }

        public double MemoryMb
        {
            get => _memoryMb;
            private set => SetProperty(ref _memoryMb, Math.Round(value, 1));
        }

        public string Uptime
        {
            get => _uptime;
            private set => SetProperty(ref _uptime, value);
        }

        public string CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        public string CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public string LastLogLine
        {
            get => _lastLogLine;
            set => SetProperty(ref _lastLogLine, value);  // Public set — called by DiagnosticsSink
        }

        /// <summary>Green ≥55 FPS, Yellow ≥30, Red below 30.</summary>
        public string FpsColor
        {
            get => _fpsColor;
            private set => SetProperty(ref _fpsColor, value);
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>Call from NavigationService.NavigateTo().</summary>
        public void SetCurrentView(string viewName, string viewModelName = "")
        {
            CurrentView = viewName;
            CurrentViewModel = string.IsNullOrEmpty(viewModelName)
                ? viewName.Replace("View", "ViewModel")
                : viewModelName;
        }

        // ── Private timer handlers ──────────────────────────────────────────────

        private void OnPollTick(object? sender, EventArgs e)
        {
            // Memory
            MemoryMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0);

            // Uptime
            var elapsed = DateTime.Now - _startTime;
            Uptime = elapsed.ToString(@"hh\:mm\:ss");
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            _frameCount++;
            var now = DateTime.Now;
            var elapsed = (now - _lastFpsCheck).TotalSeconds;

            if (elapsed >= 1.0)
            {
                var fps = _frameCount / elapsed;
                Fps = fps;
                FpsColor = fps >= 55 ? "#00FF94"   // Green
                         : fps >= 30 ? "#FFD700"   // Gold
                         : "#FF2D55";              // Red
                _frameCount = 0;
                _lastFpsCheck = now;
            }
        }

        // ── INotifyPropertyChanged ──────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }

        // ── IDisposable ─────────────────────────────────────────────────────────

        public void Dispose()
        {
            _pollTimer.Stop();
            CompositionTarget.Rendering -= OnRendering;
            Instance = null;
        }
    }

    /// <summary>
    /// Serilog sink that pipes the last log line to DiagnosticsViewModel.
    /// Register in LoggingSetup: .WriteTo.Sink(new DiagnosticsSink())
    /// </summary>
    public sealed class DiagnosticsSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            if (DiagnosticsViewModel.Instance == null) return;

            string level = logEvent.Level switch
            {
                LogEventLevel.Verbose     => "VRB",
                LogEventLevel.Debug       => "DBG",
                LogEventLevel.Information => "INF",
                LogEventLevel.Warning     => "WRN",
                LogEventLevel.Error       => "ERR",
                LogEventLevel.Fatal       => "FTL",
                _                         => "???"
            };

            string time = logEvent.Timestamp.ToString("HH:mm:ss");
            string msg  = logEvent.RenderMessage();

            // Truncate long messages for the HUD
            if (msg.Length > 80) msg = msg[..77] + "...";

            string line = $"[{time} {level}] {msg}";

            // Must dispatch to UI thread
            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                DiagnosticsViewModel.Instance.LastLogLine = line;
            });
        }
    }
}
