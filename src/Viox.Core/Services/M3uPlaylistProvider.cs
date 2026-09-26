// File: M3uPlaylistProvider.cs
namespace Viox.Core.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Core.Models;

/// <summary>
/// Provides thread-safe loading, caching, reloading, and URI lookup for M3U playlists in memory.
/// </summary>
public class M3uPlaylistProvider : IM3uPlaylistProvider
{
    private readonly IM3uPlaylistParser _parser;
    private readonly ILogger<M3uPlaylistProvider> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IClientOptionsStore _sourceLoader;

    private ConcurrentDictionary<string, string> _sources = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, MediaMetaData[]> _playlists = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, MediaMetaData> _uriIndex = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="M3uPlaylistProvider"/> class.
    /// </summary>
    /// <param name="parser">The M3U playlist parser instance.</param>
    /// <param name="logger">The logger for operational diagnostics.</param>
    public M3uPlaylistProvider(IClientOptionsStore sourceLoader, IM3uPlaylistParser parser, ILogger<M3uPlaylistProvider> logger)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourceLoader = sourceLoader ?? throw new ArgumentNullException(nameof(sourceLoader));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, MediaMetaData[]> Playlists => _playlists;

    /// <inheritdoc />
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        ClientConfiguration cfg = await _sourceLoader.GetConfigurationAsync(cancellationToken);
        IReadOnlyDictionary<string, string> sources = cfg.Playlists;
        ArgumentNullException.ThrowIfNull(sources);

        if (sources.Count() == 0)
        {
            _logger.LogWarning("No playlist sources found");
            return;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _sources = new ConcurrentDictionary<string, string>(sources, StringComparer.OrdinalIgnoreCase);
            await InternalParseAllAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_sources.IsEmpty)
            {
                _logger.LogWarning("Reload was requested, but no playlist sources have been loaded.");
                return;
            }

            _logger.LogInformation("Reloading {Count} playlist sources...", _sources.Count);
            await InternalParseAllAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        _lock.Wait();
        try
        {
            _sources.Clear();
            _playlists.Clear();
            _uriIndex.Clear();
            _logger.LogInformation("Cleared all loaded playlist data and memory indices.");
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public MediaMetaData? FindByUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return null;
        }

        if (_uriIndex.TryGetValue(uri.Trim(), out var item))
        {
            return item;
        }

        return null;
    }

    /// <inheritdoc />
    public MediaMetaData[] GetPlaylist(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Array.Empty<MediaMetaData>();
        }

        if (_playlists.TryGetValue(key, out var playlist))
        {
            return playlist;
        }

        return Array.Empty<MediaMetaData>();
    }

    private async Task InternalParseAllAsync(CancellationToken cancellationToken)
    {
        var newPlaylists = new ConcurrentDictionary<string, MediaMetaData[]>(StringComparer.OrdinalIgnoreCase);
        var newUriIndex = new ConcurrentDictionary<string, MediaMetaData>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, source) in _sources)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("Skipping playlist key '{Key}' because the source value is empty.", key);
                continue;
            }

            try
            {
                _logger.LogInformation("Parsing playlist '{Key}' from {Source}", key, source);
                MediaMetaData[] items = await ParseSourceAsync(source, cancellationToken).ConfigureAwait(false);
                newPlaylists[key] = items;

                if (items.Count() == 0)
                {
                    _logger.LogWarning("Found no items in playlist '{Key}' from {Source}", key, source);
                }

                foreach (var item in items)
                {
                    IndexMediaItem(newUriIndex, item);
                }

                _logger.LogDebug("Parsed {Count} media items for playlist key '{Key}'.", items.Length, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse playlist for key '{Key}'. Source: '{Source}'", key, source);
            }
        }

        _playlists = newPlaylists;
        _uriIndex = newUriIndex;

        _logger.LogInformation("Playlist refresh complete. Loaded {PlaylistCount} playlists containing {UriCount} unique indexed URIs.",
            _playlists.Count, _uriIndex.Count);
    }

    private async Task<MediaMetaData[]> ParseSourceAsync(string source, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(source, UriKind.Absolute, out var parsedUri) &&
            (parsedUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
             parsedUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return await _parser.ParseFromUrlAsync(source, cancellationToken).ConfigureAwait(false);
        }

        return await _parser.ParseFromFileAsync(source, cancellationToken).ConfigureAwait(false);
    }

    private static void IndexMediaItem(ConcurrentDictionary<string, MediaMetaData> index, MediaMetaData item)
    {
        index.TryAdd(item.RawUri.Trim(), item);
    }
}
