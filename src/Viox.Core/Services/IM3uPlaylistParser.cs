// File: IM3uPlaylistParser.cs
namespace Viox.Core.Services;

using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

/// <summary>
/// Service interface for parsing M3U playlists from various input sources into <see cref="MediaMetaData"/> arrays.
/// </summary>
public interface IM3uPlaylistParser
{

    /// <summary>
    /// Parses an M3U playlist asynchronously from a local file path.
    /// </summary>
    /// <param name="filePath">The absolute or relative path to the M3U file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An array of parsed <see cref="MediaMetaData"/> objects.</returns>
    Task<MediaMetaData[]> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses an M3U playlist asynchronously from a remote URL.
    /// </summary>
    /// <param name="playlistUrl">The URL pointing to the M3U playlist resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An array of parsed <see cref="MediaMetaData"/> objects.</returns>
    Task<MediaMetaData[]> ParseFromUrlAsync(string playlistUrl, CancellationToken cancellationToken = default);
}
