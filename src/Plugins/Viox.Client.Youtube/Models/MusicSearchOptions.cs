namespace Viox.Client.Youtube.Models;

public sealed class MusicSearchOptions
{
    public int Limit { get; init; } = 20;

    /// <summary>
    /// Restrict results to playable catalog items (songs and official music videos).
    /// </summary>
    public bool SongsOnly { get; init; } = true;

    /// <summary>
    /// When true, call the watch/next endpoint for each result to fill album,
    /// duration and artwork gaps. Slower but closer to complete tags.
    /// </summary>
    public bool EnrichMetadata { get; init; }

    /// <summary>
    /// When true, attempt to resolve a short-lived direct audio stream URL
    /// (via the player endpoint, then yt-dlp if available).
    /// </summary>
    public bool ResolveStreamUrl { get; init; }
}
