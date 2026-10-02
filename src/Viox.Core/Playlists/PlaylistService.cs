// File: PlaylistService.cs
using Microsoft.Extensions.Logging;
using Viox.Core.Models;

namespace Viox.Core.Playlists;

/// <summary>
/// Provides domain manipulation services for playlists.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private readonly IPlaylistRepository _repository;
    private readonly ILogger<PlaylistService> _logger;

    public PlaylistService(IPlaylistRepository repository, ILogger<PlaylistService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task AddItemToPlaylistAsync(string playlistName, MediaMetaData item, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);
        ArgumentNullException.ThrowIfNull(item);

        var playlist = await _repository.LoadPlaylistAsync(playlistName, cancellationToken) 
                       ?? new MediaMetaDataPlaylist { Title = playlistName };

        playlist.Items.Add(item);
        await _repository.SavePlaylistAsync(playlist, cancellationToken);
        
        _logger.LogInformation("Added item {ItemTitle} to playlist {PlaylistName}", item.Title, playlistName);
    }

    public async Task RemoveItemFromPlaylistAsync(string playlistName, string rawUri, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawUri);

        var playlist = await _repository.LoadPlaylistAsync(playlistName, cancellationToken);
        if (playlist is null) return;

        var removedCount = playlist.Items.RemoveAll(i => i.RawUri.Equals(rawUri, StringComparison.OrdinalIgnoreCase));
        if (removedCount > 0)
        {
            await _repository.SavePlaylistAsync(playlist, cancellationToken);
            _logger.LogInformation("Removed {Count} items matching {RawUri} from playlist {PlaylistName}", removedCount, rawUri, playlistName);
        }
    }
}
