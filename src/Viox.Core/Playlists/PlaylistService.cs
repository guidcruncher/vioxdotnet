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

    public async Task<string> CreatePlaylistAsync(string title, MediaMetaData item, CancellationToken cancellationToken)
    {
        MediaMetaDataPlaylist pl = new();
        pl.Title = title;
        pl.Id = Guid.NewGuid().ToString();
        pl.Items.Add(item);
        await _repository.SavePlaylistAsync(pl, cancellationToken);
        return pl.Id;
    }

    public async Task AddItemToPlaylistAsync(string id, MediaMetaData item, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(item);

        var playlist = await _repository.LoadPlaylistAsync(id, cancellationToken);
        if (playlist is null)
        {
            return;
        }

        playlist.Items.Add(item);
        await _repository.SavePlaylistAsync(playlist, cancellationToken);

        _logger.LogInformation("Added item {ItemTitle} to playlist {id}", item.Title, id);
    }

    public async Task RemoveItemFromPlaylistAsync(string id, string rawUri, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawUri);

        var playlist = await _repository.LoadPlaylistAsync(id, cancellationToken);
        if (playlist is null) return;

        var removedCount = playlist.Items.RemoveAll(i => i.RawUri.Equals(rawUri, StringComparison.OrdinalIgnoreCase));
        if (removedCount > 0)
        {
            await _repository.SavePlaylistAsync(playlist, cancellationToken);
            _logger.LogInformation("Removed {Count} items matching {RawUri} from playlist {id}", removedCount, rawUri, id);
        }
    }

    public async Task<MediaMetaDataPlaylist?> LoadPlaylistAsync(string name, CancellationToken cancellationToken = default) => await _repository.LoadPlaylistAsync(name, cancellationToken);

    public async Task<Dictionary<string, string>> GetPlaylistNamesAsync(CancellationToken cancellationToken = default) => await _repository.GetPlaylistNamesAsync(cancellationToken);

    public async Task<bool> DeletePlaylistAsync(string name, CancellationToken cancellationToken = default) => await _repository.DeletePlaylistAsync(name, cancellationToken);

}
