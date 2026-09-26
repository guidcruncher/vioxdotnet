namespace Viox.Server.Configuration;

using System;

/// <summary>
/// Options for configuring background podcast batch downloading.
/// </summary>
public sealed class PodcastDownloadOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "PodcastDownload";

    /// <summary>
    /// Gets or sets whether to run the background downloader immediately upon service startup.
    /// </summary>
    public bool RunOnStartup { get; set; } = false;

    /// <summary>
    /// Gets or sets the target time of day (24-hour format) for scheduled runs.
    /// Default is set to 03:00 AM.
    /// </summary>
    public TimeOnly DailyScheduleTime { get; set; } = new TimeOnly(3, 0, 0);

    /// <summary>
    /// Gets or sets the maximum parallel downloads allowed per batch cycle.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 3;

    public int MaxAgeHours { get; set; } = 336;
}
