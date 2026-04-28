namespace MavericksCopy.Models
{
    /// <summary>Log routing channels — determines which tab a line appears on.</summary>
    public enum LogChannel
    {
        All,      // every line goes here
        Errors,   // lines matched by error regex
        Stats,    // summary table lines
        Debug     // raw robocopy stdout (verbose)
    }

    /// <summary>A single parsed output line ready for display.</summary>
    public record LogLine(
        DateTime    Timestamp,
        string      Text,
        LogChannel  Channel,
        string      HexColor  // e.g. "#FF2D55" for errors, "#00C47A" for success
    );
}
