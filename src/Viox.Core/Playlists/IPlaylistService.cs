// File: IPlaylistService.cs
using Viox.Core.Models;

namespace Viox.Core.Playlists;

public interface IPlaylistService
{
    Task AddItemToPlaylistAsync(string playlistName, MediaMetaData item, CancellationToken cancellationToken = default);
    Task RemoveItemFromPlaylistAsync(string playlistName, string rawUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a playlist asynchronously by its title or file identifier.
    /// </summary>
    Task<MediaMetaDataPlaylist?> LoadPlaylistAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all stored playlist names.
    /// </summary>
    Task<IEnumerable<string>> GetPlaylistNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a playlist from the file system.
    /// </summary>
    Task<bool> DeletePlaylistAsync(string name, CancellationToken cancellationToken = default);
}
