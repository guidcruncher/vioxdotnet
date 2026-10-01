namespace Viox.Client.Youtube.Models;

/// <summary>
/// Contains the direct stream URL extracted by yt-dlp along with required HTTP header information.
/// </summary>
public sealed record YtDlpMediaStream(
    string DirectUrl,
    string UserAgent,
    TimeSpan Duration
);
