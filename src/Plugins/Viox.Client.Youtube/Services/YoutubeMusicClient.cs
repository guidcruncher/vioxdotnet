using System.Text.Json;

using Microsoft.Extensions.Logging;

using Viox.Client.Youtube.Internal;
using Viox.Client.Youtube.Models;
using Viox.Core.Services;

namespace Viox.Client.Youtube.Services;

internal sealed class YoutubeMusicClient : IYoutubeMusicClient
{
    private readonly InnerTubeClient _innerTube;
    private readonly YtDlpStreamResolver _ytDlp;
    private readonly ILogger<YoutubeMusicClient> _logger;

    public YoutubeMusicClient(
        InnerTubeClient innerTube,
        YtDlpStreamResolver ytDlp,
        ILogger<YoutubeMusicClient> logger)
    {
        _innerTube = innerTube;
        _ytDlp = ytDlp;
        _logger = logger;
    }

    public async Task<IReadOnlyList<YoutubeTrack>> SearchAsync(
        string query,
        MusicSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        options ??= new MusicSearchOptions();
        _logger.LogInformation("Searching YouTube Music for {Query}", query);
        using var initial = await _innerTube.SearchAsync(query, filterParams: null, cancellationToken).ConfigureAwait(false);
        var results = SearchResponseParser.ParseSearch(initial.RootElement).ToList();
        if (options.SongsOnly)
        {
            var songParams = SearchResponseParser.FindChipParams(initial.RootElement, "Songs");
            if (!string.IsNullOrWhiteSpace(songParams))
            {
                using var filtered = await _innerTube.SearchAsync(query, songParams, cancellationToken).ConfigureAwait(false);
                var filteredResults = SearchResponseParser.ParseSearch(filtered.RootElement);
                if (filteredResults.Count > 0)
                {
                    results = filteredResults.ToList();
                }
            }
            results = results
                .Where(r => r.Kind is YoutubeMusicResultKind.Song or YoutubeMusicResultKind.Video)
                .ToList();
        }
        if (results.Count > options.Limit)
        {
            results = results.Take(options.Limit).ToList();
        }
        if (options.EnrichMetadata || options.ResolveStreamUrl)
        {
            for (var i = 0; i < results.Count; i++)
            {
                results[i] = await EnrichAsync(results[i], options.ResolveStreamUrl, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        _logger.LogInformation("YouTube Music search for {Query} returned {Count} tracks", query, results.Count);
        return results;
    }

    public async Task<YoutubeTrack?> GetMetadataAsync(
        string videoIdOrUrl,
        bool resolveStreamUrl = false,
        CancellationToken cancellationToken = default)
    {
        var videoId = VideoIdParser.TryParse(videoIdOrUrl);
        if (videoId is null)
        {
            throw new ArgumentException("Value is not a YouTube video id or watch URL.", nameof(videoIdOrUrl));
        }
        YoutubeTrack? metadata = null;
        try
        {
            using var next = await _innerTube.NextAsync(videoId, cancellationToken).ConfigureAwait(false);
            metadata = SearchResponseParser.ParseWatchNext(videoId, next.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogWarning(ex, "Watch/next metadata failed for {VideoId}", videoId);
        }

        var uri = $"youtube:track:{videoId}";
        metadata ??= new YoutubeTrack
        {
            Uri = MediaUriParser.ParseMediaUriValue(uri),
            VideoId = videoId,
            Title = videoId,
            TrackUrl = VideoIdParser.ToTrackUrl(videoId)
        };

        return await EnrichAsync(metadata, resolveStreamUrl, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> ResolveStreamUrlAsync(
        string videoIdOrUrl,
        CancellationToken cancellationToken = default)
    {
        var videoId = VideoIdParser.TryParse(videoIdOrUrl);
        if (videoId is null)
        {
            throw new ArgumentException("Value is not a YouTube video id or watch URL.", nameof(videoIdOrUrl));
        }
        var fromPlayer = await TryPlayerStreamAsync(videoId, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(fromPlayer.StreamUrl))
        {
            return fromPlayer.StreamUrl;
        }
        return await _ytDlp.TryResolveAsync(VideoIdParser.ToTrackUrl(videoId), cancellationToken).ConfigureAwait(false);
    }

    private async Task<YoutubeTrack> EnrichAsync(
        YoutubeTrack current,
        bool resolveStream,
        CancellationToken cancellationToken)
    {
        var title = current.Title;
        var album = current.Album;
        var artists = current.Artists;
        var duration = current.Duration;
        var image = current.ImageUrl;
        var releaseDate = current.ReleaseDate;
        var albumId = current.AlbumBrowseId;
        var streamUrl = current.StreamUrl;
        var kind = current.Kind;
        if (album is null || duration is null || image is null || artists.Count == 0)
        {
            try
            {
                using var next = await _innerTube.NextAsync(current.VideoId, cancellationToken).ConfigureAwait(false);
                var parsed = SearchResponseParser.ParseWatchNext(current.VideoId, next.RootElement);
                if (parsed is not null)
                {
                    title = Prefer(title, parsed.Title, current.VideoId);
                    album ??= parsed.Album;
                    duration ??= parsed.Duration;
                    image ??= parsed.ImageUrl;
                    albumId ??= parsed.AlbumBrowseId;
                    kind = parsed.Kind == YoutubeMusicResultKind.Unknown ? kind : parsed.Kind;
                    if (artists.Count == 0 && parsed.Artists.Count > 0)
                    {
                        artists = parsed.Artists;
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                _logger.LogDebug(ex, "Optional next-endpoint enrichment failed for {VideoId}", current.VideoId);
            }
        }
        var player = await TryPlayerStreamAsync(current.VideoId, cancellationToken).ConfigureAwait(false);
        title = Prefer(title, player.Title, current.VideoId);
        duration ??= player.Duration;
        image ??= player.ImageUrl;
        releaseDate ??= player.Published;
        if (artists.Count == 0 && !string.IsNullOrWhiteSpace(player.Author))
        {
            artists = [player.Author];
        }
        if (releaseDate is null && !string.IsNullOrWhiteSpace(albumId))
        {
            try
            {
                using var browse = await _innerTube.BrowseAsync(albumId, cancellationToken).ConfigureAwait(false);
                releaseDate = SearchResponseParser.ParseAlbumYear(browse.RootElement);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                _logger.LogDebug(ex, "Album browse failed for {AlbumId}", albumId);
            }
        }
        if (resolveStream)
        {
            streamUrl = player.StreamUrl
                        ?? await _ytDlp.TryResolveAsync(current.TrackUrl, cancellationToken).ConfigureAwait(false);
        }
        return current with
        {
            Title = title,
            Album = album,
            Artists = artists,
            ReleaseDate = releaseDate,
            Duration = duration,
            ImageUrl = image,
            StreamUrl = streamUrl,
            AlbumBrowseId = albumId,
            Kind = kind
        };
    }

    private async Task<(string? Title, string? Author, TimeSpan? Duration, DateOnly? Published, string? ImageUrl, string? StreamUrl)>
        TryPlayerStreamAsync(string videoId, CancellationToken cancellationToken)
    {
        try
        {
            using var player = await _innerTube.PlayerAsync(videoId, cancellationToken).ConfigureAwait(false);
            return SearchResponseParser.ParsePlayer(player.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogDebug(ex, "Player endpoint failed for {VideoId}", videoId);
            return default;
        }
    }

    private static string Prefer(string current, string? candidate, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(current) && current != fallback)
        {
            return current;
        }
        return string.IsNullOrWhiteSpace(candidate) ? fallback : candidate;
    }
}

