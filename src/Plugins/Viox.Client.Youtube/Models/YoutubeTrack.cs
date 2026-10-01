namespace Viox.Client.Youtube.Models;

/// <summary>
/// Catalog metadata for a YouTube Music track, shaped for MPD queueing and tagging.
/// </summary>
public sealed record YoutubeTrack
{
    /// <summary>YouTube video / track id.</summary>
    public required string VideoId { get; init; }

    public required string Title { get; init; }

    public string? Album { get; init; }

    public IReadOnlyList<string> Artists { get; init; } = [];

    /// <summary>Full calendar date when known; otherwise year-only values use January 1.</summary>
    public DateOnly? ReleaseDate { get; init; }

    public TimeSpan? Duration { get; init; }

    /// <summary>Highest-resolution artwork URL available from YouTube Music.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Stable YouTube Music watch URL suitable as an MPD playlist URI when a
    /// resolver (yt-dlp, myMPD script, ffmpeg input) is configured. When
    /// <see cref="StreamUrl"/> is present this still remains the canonical page URL.
    /// </summary>
    public required string TrackUrl { get; init; }

    /// <summary>
    /// Direct audio stream URL when one could be resolved. These URLs expire
    /// quickly and usually require a User-Agent header; prefer <see cref="TrackUrl"/>
    /// plus an external resolver for long-lived MPD queues.
    /// </summary>
    public string? StreamUrl { get; init; }

    public string? AlbumBrowseId { get; init; }

    public YoutubeMusicResultKind Kind { get; init; } = YoutubeMusicResultKind.Song;

    public bool IsExplicit { get; init; }

    /// <summary>Convenience join of <see cref="Artists"/>.</summary>
    public string ArtistDisplayName => Artists.Count == 0 ? string.Empty : string.Join(", ", Artists);

    /// <summary>URI that MPD clients typically pass to <c>add</c> / <c>addid</c>.</summary>
    public string MpdUri => string.IsNullOrWhiteSpace(StreamUrl) ? TrackUrl : StreamUrl;
}
