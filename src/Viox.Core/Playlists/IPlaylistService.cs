// File: IPlaylistService.cs
using Viox.Core.Models;

namespace Viox.Core.Playlists;

public interface IPlaylistService
{
    Task AddItemToPlaylistAsync(string playlistName, MediaMetaData item, CancellationToken cancellationToken = default);
    Task RemoveItemFromPlaylistAsync(string playlistName, string rawUri, CancellationToken cancellationToken = default);
}
