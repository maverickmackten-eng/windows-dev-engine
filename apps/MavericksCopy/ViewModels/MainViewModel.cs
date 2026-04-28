using System.Collections.ObjectModel;
using System.Windows;
using MavericksCopy.Models;
using MavericksCopy.Services;
using Serilog;

namespace MavericksCopy.ViewModels
{
    /// <summary>
    /// Primary ViewModel — drives the entire MainWindow.
    ///
    /// Properties map 1:1 with the original PowerShell $app, $src, $dst,
    /// $mode, $opts, $filt, $post, $stats, $prog, $log, $btns accessors.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly RobocopyService _robocopy;
        private readonly SettingsService _settings;
        private readonly LogFileService  _logFile;

        public MainViewModel(RobocopyService robocopy,
                             SettingsService settings,
                             LogFileService  logFile)
        {
            _robocopy = robocopy;
            _settings = settings;
            _logFile  = logFile;

            // Wire robocopy events -> UI thread dispatch
            _robocopy.LineReady    += OnLineReady;
            _robocopy.StateUpdated += OnStateUpdated;
            _robocopy.Completed    += OnCompleted;

            // Commands
            StartCommand      = new AsyncRelayCommand(StartAsync,  () => !IsRunning && !string.IsNullOrWhiteSpace(Source) && !string.IsNullOrWhiteSpace(Destination));
            CancelCommand     = new RelayCommand(Cancel,           () => IsRunning);
            PauseResumeCommand= new RelayCommand(PauseResume,      () => IsRunning);
            BrowseSrcCommand  = new RelayCommand(BrowseSource);
            BrowseDstCommand  = new RelayCommand(BrowseDestination);
            ClearLogCommand   = new RelayCommand(ClearLog);
        }

        // ── Path bindings ($src / $dst) ────────────────────────────
        private string _source = "";
        private string _destination = "";

        public string Source
        {
            get => _source;
            set { SetField(ref _source, value); StartCommand.Refresh(); }
        }

        public string Destination
        {
            get => _destination;
            set { SetField(ref _destination, value); StartCommand.Refresh(); }
        }

        // ── Mode ($mode) ───────────────────────────────────────
        private bool _modeMirror;
        private bool _modeMove;
        private bool _modeDryRun;

        public bool ModeMirror  { get => _modeMirror;  set => SetField(ref _modeMirror,  value); }
        public bool ModeMove    { get => _modeMove;    set => SetField(ref _modeMove,    value); }
        public bool ModeDryRun  { get => _modeDryRun;  set => SetField(ref _modeDryRun,  value); }

        // ── Options ($opts) ─────────────────────────────────────
        private bool _subdirs    = true;
        private bool _verbose    = true;
        private bool _backupMode;
        private int  _threads    = 8;
        private int  _throttleMs;

        public bool Subdirectories { get => _subdirs;    set => SetField(ref _subdirs,    value); }
        public bool Verbose        { get => _verbose;    set => SetField(ref _verbose,    value); }
        public bool BackupMode     { get => _backupMode; set => SetField(ref _backupMode, value); }
        public int  Threads        { get => _threads;    set => SetField(ref _threads,    value); }
        public int  ThrottleMs     { get => _throttleMs; set => SetField(ref _throttleMs, value); }

        // ── Filters ($filt) ─────────────────────────────────────
        private string _excludeDirs  = "";
        private string _excludeFiles = "";

        public string ExcludeDirs  { get => _excludeDirs;  set => SetField(ref _excludeDirs,  value); }
        public string ExcludeFiles { get => _excludeFiles; set => SetField(ref _excludeFiles, value); }

        // ── Post-transfer ($post) ────────────────────────────────
        private bool _openDestination;
        public  bool OpenDestination
        {
            get => _openDestination;
            set => SetField(ref _openDestination, value);
        }

        // ── Runtime state / stats ($stats / $prog) ────────────────
        private bool    _isRunning;
        private bool    _isPaused;
        private double  _progressPercent;
        private string  _progressLabel   = "";
        private string  _etaLabel        = "";
        private string  _speedLabel      = "";
        private int     _filesCopied;
        private int     _filesFailed;
        private int     _dirsCreated;
        private string  _pauseBtnText    = "⏸ Pause";

        public bool   IsRunning       { get => _isRunning;       private set { SetField(ref _isRunning,       value); CancelCommand.Refresh(); PauseResumeCommand.Refresh(); StartCommand.Refresh(); } }
        public bool   IsPaused        { get => _isPaused;        private set => SetField(ref _isPaused,        value); }
        public double ProgressPercent { get => _progressPercent; private set => SetField(ref _progressPercent, value); }
        public string ProgressLabel   { get => _progressLabel;   private set => SetField(ref _progressLabel,   value); }
        public string EtaLabel        { get => _etaLabel;        private set => SetField(ref _etaLabel,        value); }
        public string SpeedLabel      { get => _speedLabel;      private set => SetField(ref _speedLabel,      value); }
        public int    FilesCopied     { get => _filesCopied;     private set => SetField(ref _filesCopied,     value); }
        public int    FilesFailed     { get => _filesFailed;     private set => SetField(ref _filesFailed,     value); }
        public int    DirsCreated     { get => _dirsCreated;     private set => SetField(ref _dirsCreated,     value); }
        public string PauseBtnText    { get => _pauseBtnText;    private set => SetField(ref _pauseBtnText,    value); }

        // ── Log collections ($log) ────────────────────────────────
        public ObservableCollection<LogLine> LogAll    { get; } = new();
        public ObservableCollection<LogLine> LogErrors { get; } = new();
        public ObservableCollection<LogLine> LogStats  { get; } = new();

        // ── Commands ($btns) ─────────────────────────────────────
        public AsyncRelayCommand StartCommand       { get; }
        public RelayCommand      CancelCommand      { get; }
        public RelayCommand      PauseResumeCommand { get; }
        public RelayCommand      BrowseSrcCommand   { get; }
        public RelayCommand      BrowseDstCommand   { get; }
        public RelayCommand      ClearLogCommand    { get; }

        // ── Init (called from App.OnStartup after settings load) ───────
        public void Initialize()
        {
            Source      = _settings.Current.LastSource;
            Destination = _settings.Current.LastDestination;
            Verbose     = _settings.Current.Verbose;
            BackupMode  = _settings.Current.BackupMode;
            Threads     = _settings.Current.Threads;
            ThrottleMs  = _settings.Current.ThrottleMs;
            Log.Debug("[MainVM] Initialized from settings");
        }

        // ── Start ────────────────────────────────────────────────
        private async Task StartAsync()
        {
            // Persist paths before transfer
            _settings.Current.LastSource      = Source;
            _settings.Current.LastDestination = Destination;
            _settings.Current.Verbose         = Verbose;
            _settings.Current.BackupMode      = BackupMode;
            _settings.Current.Threads         = Threads;
            _settings.Current.ThrottleMs      = ThrottleMs;
            await _settings.SaveAsync();

            var args = BuildRobocopyArgs();

            // Clear previous log
            ClearLog();

            IsRunning = true;
            _logFile.StartRun();

            // Pre-scan
            StatusMessage = "Scanning…";
            await _robocopy.PreScanAsync(args);
            StatusMessage = "Running…";

            // Real transfer
            await _robocopy.RunAsync(args);
        }

        private RobocopyArgs BuildRobocopyArgs() => new()
        {
            Source          = Source,
            Destination     = Destination,
            Mirror          = ModeMirror,
            Move            = ModeMove,
            DryRun          = ModeDryRun,
            Subdirectories  = Subdirectories,
            Verbose         = Verbose,
            BackupMode      = BackupMode,
            Threads         = Threads,
            ThrottleMs      = ThrottleMs,
            ExcludeDirs     = ExcludeDirs,
            ExcludeFiles    = ExcludeFiles,
            OpenDestination = OpenDestination,
        };

        // ── Cancel ───────────────────────────────────────────────
        private void Cancel() => _robocopy.Cancel();

        // ── Pause / Resume ────────────────────────────────────────
        private void PauseResume()
        {
            if (_robocopy.State.IsPaused)
            {
                _robocopy.Resume();
                PauseBtnText = "⏸ Pause";
                IsPaused     = false;
            }
            else
            {
                _robocopy.Pause();
                PauseBtnText = "▶ Resume";
                IsPaused     = true;
            }
        }

        // ── Browse ─────────────────────────────────────────────
        private void BrowseSource()
        {
            var path = BrowseFolder("Select Source Folder");
            if (path != null) Source = path;
        }

        private void BrowseDestination()
        {
            var path = BrowseFolder("Select Destination Folder");
            if (path != null) Destination = path;
        }

        private static string? BrowseFolder(string title)
        {
            // FolderBrowserDialog requires STA; MainWindow is always on STA thread
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description         = title,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
            };
            return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? dlg.SelectedPath
                : null;
        }

        // ── Clear log ──────────────────────────────────────────
        private void ClearLog()
        {
            LogAll.Clear();
            LogErrors.Clear();
            LogStats.Clear();
        }

        // ── Robocopy event handlers ───────────────────────────────
        private void OnLineReady(LogLine line)
        {
            // Always dispatch to UI thread
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Route to correct tab collections
                LogAll.Add(line);
                if (line.Channel == LogChannel.Errors) LogErrors.Add(line);
                if (line.Channel == LogChannel.Stats)  LogStats.Add(line);

                // Cap collections at 2000 lines each to prevent memory growth
                while (LogAll.Count    > 2000) LogAll.RemoveAt(0);
                while (LogErrors.Count > 2000) LogErrors.RemoveAt(0);
                while (LogStats.Count  > 2000) LogStats.RemoveAt(0);

                _logFile.WriteLine(line);
            });
        }

        private void OnStateUpdated(TransferState state)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressPercent = state.ProgressPercent;
                FilesCopied     = state.FilesCopied;
                FilesFailed     = state.FilesFailed;
                DirsCreated     = state.DirsCreated;

                // Speed label: e.g. "12.4 MB/s"
                double bps = state.AvgSpeedBps;
                SpeedLabel = bps switch
                {
                    >= 1_073_741_824 => $"{bps / 1_073_741_824:F1} GB/s",
                    >= 1_048_576     => $"{bps / 1_048_576:F1} MB/s",
                    >= 1_024         => $"{bps / 1_024:F1} KB/s",
                    _                => $"{bps:F0} B/s",
                };

                // ETA label
                var eta = state.EtaRemaining;
                EtaLabel = eta.HasValue
                    ? eta.Value.TotalHours >= 1
                        ? $"{(int)eta.Value.TotalHours}h {eta.Value.Minutes:D2}m"
                        : $"{eta.Value.Minutes}m {eta.Value.Seconds:D2}s"
                    : "--";

                // Progress label: "47 / 1,234 files  (3.7%)"
                ProgressLabel = state.PreScanFiles > 0
                    ? $"{state.FilesCopied:N0} / {state.PreScanFiles:N0} files  ({state.ProgressPercent:F1}%)"
                    : $"{state.FilesCopied:N0} files copied";
            });
        }

        private void OnCompleted(int exitCode)
        {
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                IsRunning     = false;
                IsPaused      = false;
                PauseBtnText  = "⏸ Pause";
                StatusMessage = ExitCodeToMessage(exitCode);
                _logFile.EndRun(exitCode);

                if (OpenDestination && System.IO.Directory.Exists(Destination))
                    System.Diagnostics.Process.Start("explorer.exe", Destination);

                Log.Information("[MainVM] Transfer complete. ExitCode={Code}", exitCode);
            });
        }

        private static string ExitCodeToMessage(int code) => code switch
        {
            0 => "No files copied (already in sync)",
            1 => "Files copied successfully",
            2 => "Extra files found in destination",
            3 => "Files copied + extra files found",
            4 => "Mismatched files found",
            5 => "Files copied + mismatches found",
            _ when code >= 8 => $"Transfer failed (exit {code})",
            _ => $"Completed (exit {code})",
        };
    }
}
