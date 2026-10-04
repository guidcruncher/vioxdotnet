// File: IPlaylistIndexService.cs
using Viox.Core.Models;

namespace Viox.Core.Playlists;

/// <summary>
/// Defines operations for maintaining and querying an in-memory index of playlists.
/// </summary>
public interface IPlaylistIndexService
{
    /// <summary>
    /// Rebuilds the in-memory index from the playlist repository.
    /// </summary>
    Task RebuildIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes or updates a single playlist in the in-memory cache.
    /// </summary>
    void IndexPlaylist(MediaMetaDataPlaylist playlist);

    /// <summary>
    /// Removes a playlist from the in-memory cache by its identifier.
    /// </summary>
    void RemovePlaylist(string playlistId);

    /// <summary>
    /// Searches for playlists matching the specified title query.
    /// </summary>
    IReadOnlyList<MediaMetaDataPlaylist> SearchByPlaylistTitle(string titleQuery);

    /// <summary>
    /// Searches for playlists that contain items matching the specified item title query.
    /// </summary>
    IReadOnlyList<MediaMetaDataPlaylist> SearchByItemTitle(string itemTitleQuery);

    /// <summary>
    /// Retrieves all playlists containing the specified RawUri.
    /// </summary>
    IReadOnlyList<MediaMetaDataPlaylist> GetPlaylistsContainingUri(string rawUri);

    /// <summary>
    /// Performs a fast check to determine if the specified RawUri exists in any indexed playlist.
    /// </summary>
    bool ContainsUri(string rawUri);
}
