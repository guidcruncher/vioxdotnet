// File: IPlaylistRepository.cs
using Viox.Core.Models;

namespace Viox.Core.Playlists;

/// <summary>
/// Defines methods for storing and retrieving playlists from the file system.
/// </summary>
public interface IPlaylistRepository
{
    /// <summary>
    /// Saves a playlist asynchronously to the file system.
    /// </summary>
    Task SavePlaylistAsync(MediaMetaDataPlaylist playlist, CancellationToken cancellationToken = default);

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
