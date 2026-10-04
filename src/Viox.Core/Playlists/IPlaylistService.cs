// File: IPlaylistService.cs
using Viox.Core.Models;

namespace Viox.Core.Playlists;

public interface IPlaylistService
{
    Task AddItemToPlaylistAsync(string id, MediaMetaData item, CancellationToken cancellationToken = default);

    Task RemoveItemFromPlaylistAsync(string rawUri, CancellationToken cancellationToken = default);

    Task<string> CreatePlaylistAsync(string title, MediaMetaData item, CancellationToken cancellationToken);

    /// <summary>
    /// Loads a playlist asynchronously by its title or file identifier.
    /// </summary>
    Task<MediaMetaDataPlaylist?> LoadPlaylistAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all stored playlist names.
    /// </summary>
    Task<Dictionary<string, string>> GetPlaylistNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a playlist from the file system.
    /// </summary>
    Task<bool> DeletePlaylistAsync(string id, CancellationToken cancellationToken = default);
}
