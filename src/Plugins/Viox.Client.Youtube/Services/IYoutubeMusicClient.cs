using Viox.Client.Youtube.Models;

namespace Viox.Client.Youtube.Services;

/// <summary>
/// Searches YouTube Music and returns catalog metadata plus a track URL that
/// can be handed to MPD (directly or through yt-dlp / a myMPD script).
/// </summary>
public interface IYoutubeMusicClient
{
    Task<IReadOnlyList<YoutubeTrack>> SearchAsync(
        string query,
        MusicSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<YoutubeTrack?> GetMetadataAsync(
        string videoIdOrUrl,
        bool resolveStreamUrl = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a short-lived audio stream URL for MPD. Returns null when
    /// neither the player endpoint nor yt-dlp can produce one.
    /// </summary>
    Task<string?> ResolveStreamUrlAsync(
        string videoIdOrUrl,
        CancellationToken cancellationToken = default);
}
