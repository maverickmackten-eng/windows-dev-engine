namespace MavericksCopy.Models
{
    /// <summary>
    /// Captures everything needed to build the robocopy command line.
    /// Passed from MainViewModel -> RobocopyService.BuildArgs().
    /// </summary>
    public class RobocopyArgs
    {
        public string Source         { get; set; } = "";
        public string Destination    { get; set; } = "";

        // Mode (mutually exclusive radio group)
        public bool   Mirror         { get; set; }        // /MIR
        public bool   Move           { get; set; }        // /MOVE
        public bool   DryRun         { get; set; }        // /L (also available as overlay on any mode)

        // Options
        public bool   Subdirectories { get; set; } = true; // /E
        public bool   Verbose        { get; set; } = true; // /V
        public bool   BackupMode     { get; set; }         // /B
        public int    Threads        { get; set; } = 8;    // /MT:N  (1-128)
        public int    ThrottleMs     { get; set; } = 0;   // /IPG:N (0 = disabled)

        // Filters
        public string ExcludeDirs    { get; set; } = "";  // semicolon-separated dir names
        public string ExcludeFiles   { get; set; } = "";  // semicolon-separated names / wildcards

        // Post-transfer
        public bool   OpenDestination { get; set; }
    }
}
