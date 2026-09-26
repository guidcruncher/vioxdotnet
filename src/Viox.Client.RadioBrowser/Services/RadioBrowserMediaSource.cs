// File: RadioBrowserMediaSource.cs
namespace Viox.Client.RadioBrowser.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.RadioBrowser.Models;
using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// RadioBrowser implementation of <see cref="IMediaSource"/> registered under key "RadioBrowser".
/// </summary>
public class RadioBrowserMediaSource : IMediaSource
{
    private readonly ILogger<RadioBrowserMediaSource> _logger;
    private readonly IRadioBrowserClient _client;

    public string Source { get => "radiobrowser"; }

    public RadioBrowserMediaSource(
        IRadioBrowserClient client,
        ILogger<RadioBrowserMediaSource> logger)
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

        Station? station = await _client.GetStationByUuidAsync(uri.Id, ct);
        if (station is null)
        {
            _logger.LogWarning("Unable to resolve metadata: RadioBrowser status or track is null.");
            return null;
        }

        MediaUri? mediaUri = station.Uri.ParseMediaUri();
        if (mediaUri is null)
        {
            _logger.LogWarning("Failed to parse MediaUri from track URI '{TrackUri}'.", station.Uri);
            return null;
        }

        MediaMetaData metaData = new()
        {
            Uri = mediaUri,
            Title = station.Name ?? string.Empty,
            Album = station.Country ?? string.Empty,
            Artist = station.State ?? string.Empty,
            Url = station.UrlResolved ?? station.Url ?? string.Empty,
            ImageUrl = station.Favicon ?? string.Empty
        };

        return metaData;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        IReadOnlyList<Station> res = await _client.GetStationsByNameExactAsync(query, options: null, cancellationToken: ct);

        List<MediaMetaData> items = res.Select(station => new MediaMetaData()
        {
            Uri = station.Uri?.ParseMediaUri(),
            Title = station.Name ?? string.Empty,
            Album = station.Country ?? string.Empty,
            Artist = station.State ?? string.Empty,
            Url = station.UrlResolved ?? station.Url ?? string.Empty,
            ImageUrl = station.Favicon ?? string.Empty
        }).ToList();

        int offset = (pageNumber - 1) * limit;
        return new PagedList<MediaMetaData>(items, offset, limit);
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();

        return res;
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }
}
