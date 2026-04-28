using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using MavericksCopy.Models;
using Serilog;

namespace MavericksCopy.Services
{
    /// <summary>
    /// Core robocopy wrapper.
    ///
    /// Responsibilities:
    ///   1. BuildArgs()   — construct the robocopy argument string from RobocopyArgs
    ///   2. PreScanAsync() — dry-run (/L) to get total file + byte counts
    ///   3. RunAsync()    — spawn robocopy, read stdout/stderr via ConcurrentQueue,
    ///                       fire LineReady events, update TransferState live
    ///   4. Pause/Resume  — NtSuspendProcess / NtResumeProcess via P/Invoke
    ///   5. Cancel()      — kill process + drain queue
    /// </summary>
    public class RobocopyService
    {
        // ── Events ─────────────────────────────────────────────
        public event Action<LogLine>?      LineReady;
        public event Action<TransferState>? StateUpdated;
        public event Action<int>?          Completed;   // robocopy exit code

        // ── State ─────────────────────────────────────────────
        public TransferState State { get; } = new();
        private Process?     _proc;
        private readonly ConcurrentQueue<string> _queue = new();
        private CancellationTokenSource?         _cts;

        // ── Regexes (compiled once) ─────────────────────────────
        // New File / Newer (increments FilesCopied + BytesTransferred)
        private static readonly Regex _rxNewFile = new(
            @"^\s*(?:New File|Newer)\s+([\d\.]+)\s+(.*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ERROR COPYING (increments FilesFailed + ErrorCount)
        private static readonly Regex _rxError = new(
            @"ERROR\s+COPYING",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // New Dir (increments DirsCreated)
        private static readonly Regex _rxNewDir = new(
            @"^\s*New Dir\s+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Summary table (Total / Copied / Skipped / Mismatch / FAILED / Extras)
        // Example: "               Files :      1234       47        0        0        0        0"
        private static readonly Regex _rxSummary = new(
            @"^\s*(Files|Dirs)\s*:\s+([\d]+)\s+([\d]+)\s+([\d]+)\s+([\d]+)\s+([\d]+)\s+([\d]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Pre-scan total line: "            Total    Copied   Skipped  Mismatch    FAILED    Extras"
        private static readonly Regex _rxPreScanFiles = new(
            @"^\s*Files\s*:\s+([\d]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _rxPreScanBytes = new(
            @"^\s*Bytes\s*:\s+([\d\.]+)\s*([kmgKMG]?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ── 1. Argument builder ───────────────────────────────────
        public static string BuildArgs(RobocopyArgs a, bool prescan = false)
        {
            // Trim trailing backslash from paths (robocopy quirk with quoted paths)
            var src  = a.Source.TrimEnd('\\');
            var dst  = a.Destination.TrimEnd('\\');

            var parts = new List<string>
            {
                $"\"{src}\"",
                $"\"{dst}\"",
            };

            // ── Always-present flags ──────────────────────────────
            parts.Add("/R:1");            // retry once on error (fast fail)
            parts.Add("/W:1");            // 1s wait between retries
            parts.Add("/NP");             // no progress percentage (we use /BYTES)
            parts.Add("/BYTES");          // report sizes in bytes
            parts.Add($"/MT:{a.Threads}"); // thread count

            // ── Conditional flags ─────────────────────────────────
            if (a.Subdirectories) parts.Add("/E");     // include empty subdirs
            if (a.Verbose)        parts.Add("/V");     // verbose output
            if (a.BackupMode)     parts.Add("/B");     // backup mode
            if (a.Mirror)         parts.Add("/MIR");   // mirror (implies /E)
            if (a.Move)           parts.Add("/MOVE");  // move (delete source after copy)
            if (a.DryRun || prescan) parts.Add("/L"); // list only (dry run / pre-scan)
            if (a.ThrottleMs > 0) parts.Add($"/IPG:{a.ThrottleMs}"); // inter-packet gap

            // ── Exclude dirs (/XD) ───────────────────────────────
            if (!string.IsNullOrWhiteSpace(a.ExcludeDirs))
            {
                var dirs = a.ExcludeDirs.Split(';', StringSplitOptions.RemoveEmptyEntries);
                parts.Add("/XD " + string.Join(" ", dirs.Select(d => $"\"{d.Trim()}\"")));
            }

            // ── Exclude files (/XF) ──────────────────────────────
            if (!string.IsNullOrWhiteSpace(a.ExcludeFiles))
            {
                var files = a.ExcludeFiles.Split(';', StringSplitOptions.RemoveEmptyEntries);
                // Wildcards (* or ?) don't get quoted
                var formatted = files.Select(f =>
                    f.Contains('*') || f.Contains('?') ? f.Trim() : $"\"{f.Trim()}\"");
                parts.Add("/XF " + string.Join(" ", formatted));
            }

            return string.Join(" ", parts);
        }

        // ── 2. Pre-scan ─────────────────────────────────────────
        /// <summary>
        /// Runs robocopy /L to count files and bytes before the real transfer.
        /// Populates State.PreScanFiles and State.PreScanBytes.
        /// </summary>
        public async Task PreScanAsync(RobocopyArgs args, CancellationToken ct = default)
        {
            var argStr = BuildArgs(args, prescan: true);
            Log.Information("[PreScan] robocopy {Args}", argStr);

            var psi = new ProcessStartInfo
            {
                FileName               = "robocopy.exe",
                Arguments              = argStr,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
            };

            using var proc = Process.Start(psi)!;
            var output = await proc.StandardOutput.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            foreach (var line in output.Split('\n'))
            {
                var mFiles = _rxPreScanFiles.Match(line);
                if (mFiles.Success)
                    State.PreScanFiles = long.Parse(mFiles.Groups[1].Value);

                var mBytes = _rxPreScanBytes.Match(line);
                if (mBytes.Success)
                {
                    double val    = double.Parse(mBytes.Groups[1].Value);
                    string suffix = mBytes.Groups[2].Value.ToUpperInvariant();
                    State.PreScanBytes = suffix switch
                    {
                        "K" => (long)(val * 1_024),
                        "M" => (long)(val * 1_048_576),
                        "G" => (long)(val * 1_073_741_824),
                        _   => (long)val,
                    };
                }
            }
            Log.Information("[PreScan] {Files} files, {Bytes} bytes",
                State.PreScanFiles, State.PreScanBytes);
        }

        // ── 3. Run ────────────────────────────────────────────
        public async Task RunAsync(RobocopyArgs args)
        {
            State.Reset();
            State.IsRunning = true;
            State.StartedAt = DateTime.UtcNow;
            _cts = new CancellationTokenSource();

            var argStr = BuildArgs(args);
            Log.Information("[RobocopyService] START: robocopy {Args}", argStr);
            EmitLine($"> robocopy.exe {argStr}", LogChannel.All, "#445566");

            var psi = new ProcessStartInfo
            {
                FileName               = "robocopy.exe",
                Arguments              = argStr,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
            };

            _proc = Process.Start(psi)!;

            // Read stdout async on background thread — enqueue lines
            _ = Task.Run(async () =>
            {
                while (!_proc.StandardOutput.EndOfStream)
                {
                    var line = await _proc.StandardOutput.ReadLineAsync();
                    if (line != null) _queue.Enqueue("O:" + line);
                }
            });

            // Read stderr
            _ = Task.Run(async () =>
            {
                while (!_proc.StandardError.EndOfStream)
                {
                    var line = await _proc.StandardError.ReadLineAsync();
                    if (line != null) _queue.Enqueue("E:" + line);
                }
            });

            // Drain queue on timer (every 500ms)
            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    DrainQueue();
                    await Task.Delay(500, _cts.Token).ContinueWith(_ => { });
                }
                DrainQueue(); // final drain
            });

            await _proc.WaitForExitAsync();
            _cts.Cancel();
            DrainQueue();

            State.ExitCode  = _proc.ExitCode;
            State.IsRunning = false;
            Completed?.Invoke(_proc.ExitCode);
            Log.Information("[RobocopyService] Exit code {Code}", _proc.ExitCode);
        }

        // ── 4. Pause / Resume (NtSuspendProcess) ─────────────────
        [System.Runtime.InteropServices.DllImport("ntdll.dll")]
        private static extern int NtSuspendProcess(IntPtr processHandle);

        [System.Runtime.InteropServices.DllImport("ntdll.dll")]
        private static extern int NtResumeProcess(IntPtr processHandle);

        public void Pause()
        {
            if (_proc == null || State.IsPaused) return;
            NtSuspendProcess(_proc.Handle);
            State.IsPaused = true;
            Log.Debug("[RobocopyService] Paused PID {Pid}", _proc.Id);
        }

        public void Resume()
        {
            if (_proc == null || !State.IsPaused) return;
            NtResumeProcess(_proc.Handle);
            State.IsPaused = false;
            Log.Debug("[RobocopyService] Resumed PID {Pid}", _proc.Id);
        }

        // ── 5. Cancel ──────────────────────────────────────────
        public void Cancel()
        {
            if (_proc == null || _proc.HasExited) return;
            if (State.IsPaused) Resume(); // must resume before killing
            State.WasCancelled = true;
            _proc.Kill(entireProcessTree: true);
            Log.Information("[RobocopyService] Cancelled");
        }

        // ── Queue drain + output parsing ──────────────────────────
        private void DrainQueue()
        {
            while (_queue.TryDequeue(out var raw))
            {
                bool isStderr = raw.StartsWith("E:");
                var  line     = raw[2..];
                ParseLine(line, isStderr);
            }
        }

        private void ParseLine(string line, bool isStderr)
        {
            var channel = LogChannel.All;
            var color   = "#8899AA"; // default: secondary text

            // ── New File / Newer ─────────────────────────────────
            var mNewFile = _rxNewFile.Match(line);
            if (mNewFile.Success)
            {
                State.FilesCopied++;
                if (long.TryParse(mNewFile.Groups[1].Value, out long sz))
                {
                    State.BytesTransferred += sz;
                    UpdateSpeedSample(sz);
                }
                // File type tracking
                var ext = Path.GetExtension(mNewFile.Groups[2].Value.Trim()).ToLowerInvariant();
                if (!string.IsNullOrEmpty(ext))
                {
                    if (!State.FileTypes.TryGetValue(ext, out var counts))
                        counts = (0, 0);
                    State.FileTypes[ext] = (counts.Copied + 1, counts.Failed);
                }
                color   = "#00C47A";
            }
            // ── Error ─────────────────────────────────────────────
            else if (_rxError.IsMatch(line) || isStderr)
            {
                State.FilesFailed++;
                State.ErrorCount++;
                channel = LogChannel.Errors;
                color   = "#FF2D55";
            }
            // ── New Dir ─────────────────────────────────────────
            else if (_rxNewDir.IsMatch(line))
            {
                State.DirsCreated++;
                color = "#00B4FF";
            }
            // ── Summary table ────────────────────────────────────
            else if (_rxSummary.IsMatch(line))
            {
                channel = LogChannel.Stats;
                color   = "#8899AA";
            }

            var logLine = new LogLine(DateTime.Now, line, channel, color);
            LineReady?.Invoke(logLine);
            StateUpdated?.Invoke(State);
        }

        // ── Speed sampling (rolling 8-sample buffer) ───────────────
        private void UpdateSpeedSample(long newBytes)
        {
            var now     = DateTime.UtcNow;
            var elapsed = (now - State.LastSnapTime).TotalSeconds;
            if (elapsed < 0.1) return; // skip sub-100ms intervals

            double bps = newBytes / elapsed;
            State.SpeedSamples[State.SpeedIndex % 8] = bps;
            State.SpeedIndex++;
            State.LastSnapTime = now;
            State.LastByteSnap = State.BytesTransferred;
        }

        // ── Log helper ──────────────────────────────────────────
        private void EmitLine(string text, LogChannel ch, string color)
            => LineReady?.Invoke(new LogLine(DateTime.Now, text, ch, color));
    }
}
