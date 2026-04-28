using System.IO;
using MavericksCopy.Models;
using Serilog;

namespace MavericksCopy.Services
{
    /// <summary>
    /// Writes robocopy output lines to per-run log files.
    ///
    /// File naming: logs/transfer_YYYYMMDD_HHmmss.log
    /// Auto-logger:  logs/auto_YYYYMMDD.log  (WARN/ERROR only, appended across runs)
    ///
    /// Both files are plain UTF-8 text, one line per entry.
    /// The auto-logger file is intended for Watch-Transfers-style background monitoring.
    /// </summary>
    public class LogFileService
    {
        private static readonly string _logsDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private StreamWriter? _runLog;
        private string        _autoLogPath = "";

        public string? CurrentRunLogPath { get; private set; }

        public void StartRun()
        {
            Directory.CreateDirectory(_logsDir);
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            CurrentRunLogPath = Path.Combine(_logsDir, $"transfer_{stamp}.log");
            _runLog           = new StreamWriter(CurrentRunLogPath, append: false,
                                                  System.Text.Encoding.UTF8) { AutoFlush = true };
            _autoLogPath      = Path.Combine(_logsDir,
                $"auto_{DateTime.Now:yyyyMMdd}.log");
            Log.Debug("[LogFile] Run log: {Path}", CurrentRunLogPath);
        }

        public void WriteLine(LogLine line)
        {
            _runLog?.WriteLine($"[{line.Timestamp:HH:mm:ss.fff}] {line.Text}");

            // Auto-logger only captures warnings and errors
            if (line.Channel == LogChannel.Errors)
            {
                try
                {
                    File.AppendAllText(_autoLogPath,
                        $"[{line.Timestamp:yyyy-MM-dd HH:mm:ss}] [ERROR] {line.Text}{Environment.NewLine}",
                        System.Text.Encoding.UTF8);
                }
                catch { /* never crash on log write */ }
            }
        }

        public void EndRun(int exitCode)
        {
            _runLog?.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] EXIT CODE: {exitCode}");
            _runLog?.Dispose();
            _runLog = null;
            Log.Debug("[LogFile] Run ended, exit {Code}", exitCode);
        }
    }
}
