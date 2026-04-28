namespace MavericksCopy.Models
{
    /// <summary>
    /// Captures everything needed to build the robocopy command line.
    /// Passed from MainViewModel → RobocopyService.BuildArgs().
    /// </summary>
    public class RobocopyArgs
    {
        public string  Source          { get; set; } = "";
        public string  Destination     { get; set; } = "";

        // Mode (mutually exclusive)
        public bool    Mirror          { get; set; }   // /MIR
        public bool    Move            { get; set; }   // /MOVE
        public bool    DryRun          { get; set; }   // /L

        // Options
        public bool    Subdirectories  { get; set; } = true;  // /E
        public bool    Verbose         { get; set; } = true;  // /V
        public bool    BackupMode      { get; set; }          // /B
        public int     Threads         { get; set; } = 8;     // /MT:N
        public int     ThrottleMs      get; set; } = 0;      // /IPG:N (0 = off)

        // Filters
        public string  ExcludeDirs     { get; set; } = "";   // comma-separated
        public string  ExcludeFiles    { get; set; } = "";   // comma-separated / wildcards

        // Post-transfer
        public bool    OpenDestination { get; set; }
    }
}
