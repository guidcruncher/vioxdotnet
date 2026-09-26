namespace Viox.Core.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Core.Models;

/// <summary>
/// RadioBrowser implementation of <see cref="IMediaSource"/> registered under key "RadioBrowser".
/// </summary>
public class M3uPlaylistMediaSource : IMediaSource
{
    private readonly ILogger<M3uPlaylistMediaSource> _logger;
    private readonly IM3uPlaylistProvider _client;

    public string Source { get => "playlist"; }

    public M3uPlaylistMediaSource(
        IM3uPlaylistProvider client,
        ILogger<M3uPlaylistMediaSource> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _logger = logger;
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct = default)
    {
        if (uri?.Id is null)
        {
            return null;
        }

        if (_client.Playlists.Count() == 0) { await _client.LoadAsync(ct); }
        foreach (string key in _client.Playlists.Keys)
        {
            foreach (MediaMetaData m in _client.Playlists[key])
            {
                if (m.RawUri == uri.ToString())
                {
                    return m;
                }
            }
        }

        return null;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        if (_client.Playlists.Count() == 0) { await _client.LoadAsync(ct); }
        List<MediaMetaData> items = _client.Playlists.Values
                .SelectMany(tracks => tracks)
            .Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        int offset = (pageNumber - 1) * limit;
        return new PagedList<MediaMetaData>(items, offset, limit);
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        if (_client.Playlists.Count() == 0) { await _client.LoadAsync(cancellationToken); }
        List<MediaMetaData> items = _client.Playlists.Values
            .SelectMany(tracks => tracks)
            .ToList();
        return items;

    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }
}
