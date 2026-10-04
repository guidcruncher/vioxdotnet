// File: PlaylistIndexService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Models;
using Viox.Core.Configuration;

namespace Viox.Core.Playlists;

/// <summary>
/// Thread-safe in-memory indexing service providing fast searches and URI existence checks.
/// </summary>
public class PlaylistIndexService : IPlaylistIndexService, IDisposable
{
    private readonly IPlaylistRepository _repository;
    private readonly ILogger<PlaylistIndexService> _logger;
    private readonly PlaylistOptions _options;
    private readonly ReaderWriterLockSlim _lock = new();

    private readonly Dictionary<string, MediaMetaDataPlaylist> _playlists = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _uriToPlaylistIdsMap = new(StringComparer.OrdinalIgnoreCase);

    private bool _disposed;

    public PlaylistIndexService(
        IPlaylistRepository repository,
        IOptions<PlaylistOptions> options,
        ILogger<PlaylistIndexService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting playlist index rebuild...");

        var playlistNames = await _repository.GetPlaylistNamesAsync(cancellationToken);
        var loadedPlaylists = new List<MediaMetaDataPlaylist>(playlistNames.Count);

        foreach (var id in playlistNames.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var playlist = await _repository.LoadPlaylistAsync(id, cancellationToken);
            if (playlist is not null)
            {
                loadedPlaylists.Add(playlist);
            }
        }

        _lock.EnterWriteLock();
        try
        {
            _playlists.Clear();
            _uriToPlaylistIdsMap.Clear();

            foreach (var playlist in loadedPlaylists)
            {
                AddPlaylistToStateUnsafe(playlist);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _logger.LogInformation("Playlist index rebuild complete. Total indexed playlists: {Count}", loadedPlaylists.Count);
    }

    public void IndexPlaylist(MediaMetaDataPlaylist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        _lock.EnterWriteLock();
        try
        {
            if (_playlists.TryGetValue(playlist.Id, out var existing))
            {
                RemovePlaylistFromStateUnsafe(existing.Id);
            }

            AddPlaylistToStateUnsafe(playlist);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _logger.LogDebug("Indexed playlist {PlaylistId} ({PlaylistTitle})", playlist.Id, playlist.Title);
    }

    public void RemovePlaylist(string playlistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        _lock.EnterWriteLock();
        try
        {
            RemovePlaylistFromStateUnsafe(playlistId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _logger.LogDebug("Removed playlist {PlaylistId} from index", playlistId);
    }

    public IReadOnlyList<MediaMetaDataPlaylist> SearchByPlaylistTitle(string titleQuery)
    {
        if (string.IsNullOrWhiteSpace(titleQuery))
        {
            return Array.Empty<MediaMetaDataPlaylist>();
        }

        _lock.EnterReadLock();
        try
        {
            return _playlists.Values
                .Where(p => p.Title.Contains(titleQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<MediaMetaDataPlaylist> SearchByItemTitle(string itemTitleQuery)
    {
        if (string.IsNullOrWhiteSpace(itemTitleQuery))
        {
            return Array.Empty<MediaMetaDataPlaylist>();
        }

        _lock.EnterReadLock();
        try
        {
            return _playlists.Values
                .Where(p => p.Items.Any(item => item.Title.Contains(itemTitleQuery, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<MediaMetaDataPlaylist> GetPlaylistsContainingUri(string rawUri)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return Array.Empty<MediaMetaDataPlaylist>();
        }

        _lock.EnterReadLock();
        try
        {
            if (!_uriToPlaylistIdsMap.TryGetValue(rawUri, out var playlistIds) || playlistIds.Count == 0)
            {
                return Array.Empty<MediaMetaDataPlaylist>();
            }

            var results = new List<MediaMetaDataPlaylist>(playlistIds.Count);
            foreach (var id in playlistIds)
            {
                if (_playlists.TryGetValue(id, out var playlist))
                {
                    results.Add(playlist);
                }
            }

            return results;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool ContainsUri(string rawUri)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return false;
        }

        _lock.EnterReadLock();
        try
        {
            return _uriToPlaylistIdsMap.TryGetValue(rawUri, out var playlistIds) && playlistIds.Count > 0;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private void AddPlaylistToStateUnsafe(MediaMetaDataPlaylist playlist)
    {
        _playlists[playlist.Id] = playlist;

        foreach (var item in playlist.Items)
        {
            if (string.IsNullOrWhiteSpace(item.RawUri))
            {
                continue;
            }

            if (!_uriToPlaylistIdsMap.TryGetValue(item.RawUri, out var playlistSet))
            {
                playlistSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _uriToPlaylistIdsMap[item.RawUri] = playlistSet;
            }

            playlistSet.Add(playlist.Id);
        }
    }

    private void RemovePlaylistFromStateUnsafe(string playlistId)
    {
        if (!_playlists.Remove(playlistId, out var existingPlaylist))
        {
            return;
        }

        foreach (var item in existingPlaylist.Items)
        {
            if (string.IsNullOrWhiteSpace(item.RawUri))
            {
                continue;
            }

            if (_uriToPlaylistIdsMap.TryGetValue(item.RawUri, out var playlistSet))
            {
                playlistSet.Remove(playlistId);
                if (playlistSet.Count == 0)
                {
                    _uriToPlaylistIdsMap.Remove(item.RawUri);
                }
            }
        }
    }

    public void Dispose()
    {

        if (_disposed)
        {
            return;
        }

        _lock.Dispose();
        _disposed = true;
    }
}
