namespace Viox.Client.Youtube.Configuration;

/// <summary>
/// Configuration options for yt-dlp CLI execution and stream extraction.
/// </summary>
public sealed class YtDlpOptions
{
    public const string SectionName = "YtDlp";

    /// <summary>
    /// Gets or sets the path to the yt-dlp executable.
    /// Default assumes it is present in system PATH.
    /// </summary>
    public string ExecutablePath { get; set; } = "yt-dlp";

    /// <summary>
    /// Gets or sets the default output directory for downloaded media files.
    /// </summary>
    public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Gets or sets the execution timeout duration for process execution.
    /// </summary>
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(10);
}
