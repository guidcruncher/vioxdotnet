// File: IM3uPlaylistProvider.cs
namespace Viox.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

/// <summary>
/// Service interface for managing, caching, reloading, and searching in-memory M3U playlists.
/// </summary>
public interface IM3uPlaylistProvider
{
    /// <summary>
    /// Gets the dictionary of currently loaded playlists keyed by their assigned identifier.
    /// </summary>
    IReadOnlyDictionary<string, MediaMetaData[]> Playlists { get; }

    /// <summary>
    /// Loads and parses a dictionary of playlist sources.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reloads all registered playlist sources from their original locations or content definitions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ReloadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all loaded playlist data and search indices from memory.
    /// </summary>
    void Clear();

    /// <summary>
    /// Searches for a media item across all loaded playlists matching the given URI string (Url, RawUri, or Uri).
    /// </summary>
    /// <param name="uri">The string URI value to find.</param>
    /// <returns>The matching <see cref="MediaMetaData"/> if found; otherwise, <c>null</c>.</returns>
    MediaMetaData? FindByUri(string uri);

    /// <summary>
    /// Retrieves the media items associated with a specific playlist key.
    /// </summary>
    /// <param name="key">The key identifying the playlist.</param>
    /// <returns>An array of <see cref="MediaMetaData"/> items for the playlist, or an empty array if not found.</returns>
    MediaMetaData[] GetPlaylist(string key);
}
