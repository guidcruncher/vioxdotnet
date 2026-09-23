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

}
