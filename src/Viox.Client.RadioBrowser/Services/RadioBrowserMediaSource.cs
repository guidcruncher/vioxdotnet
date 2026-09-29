namespace Viox.Client.RadioBrowser.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.RadioBrowser.Models;
using Viox.Core.Models;
using Viox.Core.Plugins;
using Viox.Core.Services;

/// <summary>
/// RadioBrowser implementation of <see cref="IMediaSource"/> registered under key "RadioBrowser".
/// </summary>
public class RadioBrowserMediaSource : IMediaSource
{
    private readonly ILogger<RadioBrowserMediaSource> _logger;
    private readonly IRadioBrowserClient _client;
    private readonly IMemoryCacheService<List<MediaMetaData>> _cache;
    private readonly MediaMetaDataConverterResolver _resolver;

    public string Source => "radiobrowser";
    public string Title => "RadioBrowser";

    public Dictionary<string, string> Props { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Title"] = "Radio Browser",
        ["Icon"] = "",
        ["Url"] = "/radio?id=:defaultCountry"
    };

    public RadioBrowserMediaSource(
        IRadioBrowserClient client,
        IMemoryCacheService<List<MediaMetaData>> cache,
        ILogger<RadioBrowserMediaSource> logger,
        MediaMetaDataConverterResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _resolver = resolver;
        _cache = cache;
        _logger = logger;
    }

    private static bool IsNumeric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return double.TryParse(value, out _);
    }

    private List<MediaMetaData> GetCountries()
    {
        Dictionary<string, string> source = IsoCountryCodes.GetIso3166Codes();
        List<MediaMetaData> res = new();

        Dictionary<string, string> sorted = source
            .Where(kvp => !IsNumeric(kvp.Key))
            .OrderBy(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (string key in sorted.Keys)
        {
            MediaUri uri = new()
            {
                Source = "radiobrowser",
                Type = "link",
                Id = key
            };

            res.Add(new()
            {
                Uri = uri,
                Album = "",
                Artist = "",
                Url = key,
                Title = sorted[key],
                ImageUrl = "/radiobrowser.png"
            });
        }

        return res;
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

        MediaMetaData? metaData = _resolver.Convert(station);
        return metaData;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        IReadOnlyList<Station> res = await _client.GetStationsByNameExactAsync(query, options: null, cancellationToken: ct);

        List<MediaMetaData> items = res
            .Select(station => _resolver.Convert(station))
            .OfType<MediaMetaData>()
            .ToList();

        int offset = (pageNumber - 1) * limit;
        return new PagedList<MediaMetaData>(items, offset, limit);
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();
        string id = parameters?.GetValueOrDefault("id")?.ToString() ?? "";

        if (string.IsNullOrEmpty(id))
        {
            return GetCountries();
        }

        IReadOnlyList<Station> stations = await _client.GetStationsByCountryCodeAsync(id, null, cancellationToken);
        if (stations is null)
        {
            return res;
        }

        res = _resolver.ConvertList(stations)
            .OfType<MediaMetaData>()
            .ToList();

        return res;
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }
}
