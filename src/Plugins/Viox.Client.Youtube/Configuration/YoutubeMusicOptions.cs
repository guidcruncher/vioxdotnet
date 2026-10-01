namespace Viox.Client.Youtube.Configuration;

public sealed class YoutubeMusicOptions
{
    public const string SectionName = "YoutubeMusic";

    /// <summary>InnerTube client name used by the YouTube Music web app.</summary>
    public string ClientName { get; set; } = "WEB_REMIX";

    /// <summary>
    /// Client version. When empty the library reads the current value from
    /// music.youtube.com on first use.
    /// </summary>
    public string? ClientVersion { get; set; }

    public string Language { get; set; } = "en";

    /// <summary>ISO 3166-1 alpha-2 region used for catalog availability.</summary>
    public string Region { get; set; } = "US";

    /// <summary>Override the public WEB_REMIX API key if YouTube rotates it.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Optional visitorData cookie/value forwarded in the InnerTube context.</summary>
    public string? VisitorData { get; set; }

    /// <summary>
    /// Path to yt-dlp (or youtube-dl) used as a fallback stream resolver for MPD.
    /// When null the library looks for <c>yt-dlp</c> on PATH.
    /// </summary>
    public string? YtDlpPath { get; set; } = "yt-dlp";

    public bool EnableYtDlpFallback { get; set; } = true;

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(20);
}
