namespace Viox.Client.Youtube.Services;

using Viox.Client.Youtube.Models;

/// <summary>
/// Service contract for extracting direct streaming links from YouTube URLs for MPD playback.
/// </summary>
public interface IYtDlpStreamExtractor
{
    /// <summary>
    /// Executes yt-dlp to extract the direct media URL and HTTP headers needed by MPD.
    /// </summary>
    /// <param name="youtubeUrl">The web URL of the YouTube video/audio.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A model containing the extracted media URL and User-Agent.</returns>
    Task<YtDlpMediaStream> ExtractStreamAsync(string youtubeUrl, CancellationToken cancellationToken = default);

}
