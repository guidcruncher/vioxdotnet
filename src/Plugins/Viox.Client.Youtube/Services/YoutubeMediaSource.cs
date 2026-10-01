// File: YoutubeMediaSource.cs
namespace Viox.Client.Youtube.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Youtube.Models;
using Viox.Core.Models;
using Viox.Core.Plugins;
using Viox.Core.Services;

/// <summary>
/// Youtube implementation of <see cref="IMediaSource"/> registered under key "Youtube".
/// </summary>
public class YoutubeMediaSource : IMediaSource
{
    private readonly ILogger<YoutubeMediaSource> _logger;
    private readonly IYoutubeMusicClient _client;
    private readonly MediaMetaDataConverterResolver _resolver;
    private readonly IMemoryCacheService<List<MediaMetaData>> _cache;
    private readonly IYtDlpStreamExtractor _ytdlp;
    public string Source { get => "youtube"; }
    public string Title => "Youtube Music";

    public Dictionary<string, string> Props { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Title"] = "Youtube Music",
        ["Icon"] = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxMDAgMTAwIiB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0ibWF4LXdpZHRoOiAxMDI0cHg7IG1heC1oZWlnaHQ6IDEwMjRweDsiPiA8IS0tIFJlZCBiYWNrZ3JvdW5kIGNpcmNsZSAtLT4gPGNpcmNsZSBjeD0iNTAiIGN5PSI1MCIgcj0iNDgiIGZpbGw9IiNGRjAwMDAiIC8+IDwhLS0gT3V0ZXIgcmluZyAtLT4gPGNpcmNsZSBjeD0iNTAiIGN5PSI1MCIgcj0iMjgiIGZpbGw9Im5vbmUiIHN0cm9rZT0iI0ZGRkZGRiIgc3Ryb2tlLXdpZHRoPSI2IiAvPiA8IS0tIENlbnRyYWwgcGxheSBidXR0b24gLS0+IDxwb2x5Z29uIHBvaW50cz0iNDQsMzggNDQsNjIgNjIsNTAiIGZpbGw9IiNGRkZGRkYiIC8+IDwvc3ZnPg==",
        ["Url"] = ""
    };

    public YoutubeMediaSource(
        IYtDlpStreamExtractor ytdlp,
        IYoutubeMusicClient client,
        ILogger<YoutubeMediaSource> logger,
        IMemoryCacheService<List<MediaMetaData>> cache,
        MediaMetaDataConverterResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(ytdlp);

        _ytdlp = ytdlp;
        _cache = cache;
        _client = client;
        _resolver = resolver;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken cancellationToken = default)
    {
        if (uri is null)
        {
            return null;
        }


        bool existsBefore = await _cache.ExistsAsync("youtube.search", cancellationToken);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync("youtube.search", cancellationToken);
            if (cachedRes is not null)
            {
                return cachedRes.FirstOrDefault(t => t.RawUri == $"youtube:track:{uri.Id}");
            }
        }
        return null;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        string key = $"youtube-search-{query}";
        bool existsBefore = await _cache.ExistsAsync(key, ct);
        if (existsBefore)
        {
            List<MediaMetaData>? cachedRes = await _cache.GetAsync(key, ct);
            if (cachedRes is not null)
            {
                return new PagedList<MediaMetaData>(cachedRes, cachedRes.Count(), 0, limit);
            }
        }

        var results = await _client.SearchAsync(query, new MusicSearchOptions
        {
            Limit = limit,
            SongsOnly = true,
            EnrichMetadata = true
        });

        if (results is null)
        {
            return new PagedList<MediaMetaData>();
        }

        var items = _resolver.ConvertList(results);
        await _cache.SetAsync(key, items.ToList(), absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: ct);
        await _cache.SetAsync("youtube.search", items.ToList(), absoluteExpiration: TimeSpan.FromHours(1), cancellationToken: ct);
        return new PagedList<MediaMetaData>(items, items.Count(), 0, limit);
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        return new List<MediaMetaData>();
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }

    public async Task<string> GetPlaybackUrl(MediaMetaData input, CancellationToken ct = default)
    {
        var ytUrl = await _ytdlp.ExtractStreamAsync(input.Url, ct);

        if (ytUrl is null)
        {
            _logger.LogError("Unable to resolve Youtube URL for {Uri}", input.RawUri);
            return input.Url;
        }

        _logger.LogInformation("Uri {RawUri} - Extracted Direct URL: {Url}", input.RawUri, ytUrl.DirectUrl);
        _logger.LogInformation("Uri {RawUri} - Extracted User-Agent: {UserAgent}", input.RawUri, ytUrl.UserAgent);
        return ytUrl.DirectUrl;
    }
}
