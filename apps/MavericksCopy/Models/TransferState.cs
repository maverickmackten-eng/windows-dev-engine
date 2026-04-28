namespace MavericksCopy.Models
{
    /// <summary>
    /// Live runtime state for a single robocopy operation.
    /// Populated by RobocopyService as output lines arrive.
    /// All counts mirror the PowerShell $State object.
    /// </summary>
    public class TransferState
    {
        // ── Process bookkeeping ──────────────────────────────────
        public bool     IsRunning     { get; set; }
        public bool     IsPaused      { get; set; }
        public bool     WasCancelled  { get; set; }
        public DateTime? StartedAt    { get; set; }

        // ── Pre-scan totals (from /L dry-run pass) ───────────────
        public long PreScanFiles      { get; set; }
        public long PreScanBytes      { get; set; }

        // ── Live transfer counts ───────────────────────────────
        public int  FilesCopied       { get; set; }
        public int  FilesSkipped      { get; set; }
        public int  FilesFailed       { get; set; }
        public int  FilesExtra        { get; set; }
        public int  DirsCreated       { get; set; }
        public int  ErrorCount        { get; set; }

        // ── Byte tracking ───────────────────────────────────
        public long BytesTransferred  { get; set; }

        // ── Speed sampling (rolling 8-sample buffer) ──────────────
        /// <summary>Circular buffer of recent bytes/sec samples.</summary>
        public double[] SpeedSamples  { get; set; } = new double[8];
        public int      SpeedIndex    { get; set; }
        public long     LastByteSnap  { get; set; }
        public DateTime LastSnapTime  { get; set; } = DateTime.UtcNow;

        /// <summary>Computed average speed across the rolling buffer (bytes/sec).</summary>
        public double   AvgSpeedBps   => SpeedSamples.Where(s => s > 0).DefaultIfEmpty(0).Average();

        // ── File type breakdown ────────────────────────────────
        /// <summary>Key = extension (e.g. ".pdf"), Value = (copied, failed) counts.</summary>
        public Dictionary<string, (int Copied, int Failed)> FileTypes { get; set; } = new();

        // ── Robocopy exit code (set on process exit) ────────────
        public int? ExitCode          { get; set; }

        // ── Helpers ──────────────────────────────────────────
        public double ProgressPercent =>
            PreScanBytes > 0 ? Math.Min(100.0, BytesTransferred * 100.0 / PreScanBytes) : 0;

        public TimeSpan? EtaRemaining
        {
            get
            {
                if (PreScanBytes <= 0 || AvgSpeedBps <= 0) return null;
                long remaining = PreScanBytes - BytesTransferred;
                return TimeSpan.FromSeconds(remaining / AvgSpeedBps);
            }
        }

        public void Reset()
        {
            IsRunning = IsPaused = WasCancelled = false;
            StartedAt = null;
            PreScanFiles = PreScanBytes = BytesTransferred = 0;
            FilesCopied = FilesSkipped = FilesFailed = FilesExtra = DirsCreated = ErrorCount = 0;
            SpeedSamples = new double[8]; SpeedIndex = 0;
            LastByteSnap = 0; LastSnapTime = DateTime.UtcNow;
            FileTypes.Clear();
            ExitCode = null;
        }
    }
}
