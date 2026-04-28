namespace MavericksCopy.Models
{
    /// <summary>
    /// All keys persisted in settings.json.
    /// Matches the PowerShell original exactly (camelCase JSON, typed C# props).
    /// </summary>
    public class AppSettings
    {
        // ── Path memory ────────────────────────────────────────
        /// <summary>Last used source path.</summary>
        public string LastSource      { get; set; } = "";
        /// <summary>Last used destination path.</summary>
        public string LastDestination { get; set; } = "";

        // ── Toggle state ─────────────────────────────────────
        /// <summary>Verbose output checkbox (/V flag).</summary>
        public bool Verbose           { get; set; } = true;
        /// <summary>Enable backup mode (/B flag).</summary>
        public bool BackupMode        { get; set; } = false;

        // ── Numeric ─────────────────────────────────────────
        /// <summary>Thread count for /MT:N (1-128, default 8).</summary>
        public int  Threads           { get; set; } = 8;
        /// <summary>Inter-packet delay in milliseconds (/IPG:N).</summary>
        public int  ThrottleMs        { get; set; } = 0;

        // ── Reserved / future ────────────────────────────────
        /// <summary>Named preset storage (key = preset name, value = serialized args).</summary>
        public Dictionary<string, string> Presets { get; set; } = new();
    }
}
