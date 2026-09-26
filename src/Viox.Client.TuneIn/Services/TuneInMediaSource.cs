using Microsoft.Extensions.Logging;

using Viox.Client.TuneIn.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.TuneIn.Services;

/// <summary>
/// TuneIn implementation of <see cref="IMediaSource"/> registered under key "TuneIn".
/// </summary>
public class TuneInMediaSource : IMediaSource
{
    private readonly ILogger<TuneInMediaSource> _logger;
    private readonly ITuneInClient _client;

    public string Source { get => "tunein"; }

    public TuneInMediaSource(
        ITuneInClient client,
        ILogger<TuneInMediaSource> logger)
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

        TuneInResponse<StationElement>? rStation = await _client.DescribeAsync(uri.Id, ct);
        TuneInResponse<AudioElement>? rAudio = await _client.TuneAsync(uri.Id, null, ct);

        if (rStation?.Body is null || rAudio?.Body is null)
        {
            return null;
        }

        StationElement? station = rStation.Body.FirstOrDefault();
        AudioElement? audio = rAudio.Body.FirstOrDefault();

        if (station is null || audio is null)
        {
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
            Album = station.Slogan ?? string.Empty,
            Artist = station.CallSign ?? string.Empty,
            Url = audio.Url ?? string.Empty,
            ImageUrl = station.Logo ?? string.Empty
        };

        return metaData;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        TuneInResponse<TuneInOutline>? res = await _client.SearchAsync(query, ct);

        if (res is null || res.Body is null)
        {
            return new PagedList<MediaMetaData>();
        }

        List<MediaMetaData> items = res.Body.Where(t => t.Type == "audio").Select(station => new MediaMetaData()
        {
            Uri = station.Uri?.ParseMediaUri(),
            Title = station.Text ?? string.Empty,
            Album = string.IsNullOrEmpty(station.Playing) ? string.Empty : station.Playing,
            Artist = station.Subtext is not null ? station.Subtext : string.Empty,
            Url = string.IsNullOrEmpty(station.Url) ? string.Empty : station.Url,
            ImageUrl = string.IsNullOrEmpty(station.Image) ? string.Empty : station.Image
        }).ToList();
        return new PagedList<MediaMetaData>(items, (pageNumber * limit), limit);
    }

    private List<MediaMetaData> ParseOutlineList(List<TuneInOutline> outlines)
    {
        List<MediaMetaData> res = new();
        foreach (TuneInOutline outline in outlines)
        {
            if (outline.Key == "stations")
            {
                if (outline.Children is not null)
                {
                    List<MediaMetaData> stations = ParseOutlineList(outline.Children);
                    if (stations.Any())
                    {
                        res.AddRange(stations);
                    }
                }
                continue;
            }

            if (outline.Type is not null)
            {
                string uri = outline.Type switch
                {
                    "audio" => $"tunein:station:{outline.GuideId}",
                    "link" => $"tunein:link:{outline.GuideId}",
                    _ => $"tunein:{outline.Type}:{outline.GuideId}"
                };

                MediaMetaData metaData = new()
                {
                    Uri = MediaUriParser.ParseMediaUriValue(uri),
                    Title = outline.Text ?? "",
                    ImageUrl = outline.Image ?? "",
                    Album = "",
                    Artist = "",
                    Url = outline.Url ?? ""
                };

                res.Add(metaData);
            }
        }
        return res;
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();
        string id = parameters?.GetValueOrDefault("id")?.ToString() ?? "r0";
        TuneInResponse<TuneInOutline>? response = await _client.BrowseAsync(null, id, null, cancellationToken);

        if (response is null)
        {
            return res;
        }

        if (response.Body is not null)
        {
            res = ParseOutlineList(response.Body);
        }

        return res;
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }
}
