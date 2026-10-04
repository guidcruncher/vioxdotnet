// File: PlaylistService.cs
using Microsoft.Extensions.Logging;

using Viox.Core.Models;

namespace Viox.Core.Playlists;

/// <summary>
/// Provides domain manipulation services for playlists and maintains the in-memory index.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private readonly IPlaylistRepository _repository;
    private readonly IPlaylistIndexService _indexService;
    private readonly ILogger<PlaylistService> _logger;

    public PlaylistService(
        IPlaylistRepository repository,
        IPlaylistIndexService indexService,
        ILogger<PlaylistService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> CreatePlaylistAsync(string title, MediaMetaData item, CancellationToken cancellationToken = default)
    {
        MediaMetaDataPlaylist pl = new()
        {
            Title = title,
            Id = Guid.NewGuid().ToString()
        };
        pl.Items.Add(item);

        await _repository.SavePlaylistAsync(pl, cancellationToken);
        _indexService.IndexPlaylist(pl);

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
        _indexService.IndexPlaylist(playlist);

        _logger.LogInformation("Added item {ItemTitle} to playlist {id}", item.Title, id);
    }

    public async Task RemoveItemFromPlaylistAsync(string rawUri, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawUri);
        int removedCount = 0;
        Dictionary<string, string> playlists = await GetPlaylistNamesAsync(cancellationToken);

        foreach (string key in playlists.Keys)
        {
            var playlist = await _repository.LoadPlaylistAsync(key, cancellationToken);
            if (playlist is null)
            {
                continue;
            }

            var listRemovedCount = playlist.Items.RemoveAll(i => i.RawUri.Equals(rawUri, StringComparison.OrdinalIgnoreCase));
            if (listRemovedCount > 0)
            {
                await _repository.SavePlaylistAsync(playlist, cancellationToken);
                removedCount += listRemovedCount;
                _indexService.IndexPlaylist(playlist);
                _logger.LogInformation("Removed {Count} items matching {RawUri} from playlist {id}", removedCount, rawUri, key);
            }
        }

        if (removedCount > 0)
        {
        }

    }

    public async Task<MediaMetaDataPlaylist?> LoadPlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.LoadPlaylistAsync(id, cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetPlaylistNamesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlaylistNamesAsync(cancellationToken);
    }

    public async Task<bool> DeletePlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var deleted = await _repository.DeletePlaylistAsync(id, cancellationToken);
        if (deleted)
        {
            _indexService.RemovePlaylist(id);
        }

        return deleted;
    }
}
