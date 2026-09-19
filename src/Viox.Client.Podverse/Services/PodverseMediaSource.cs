// File: PodverseMediaSource.cs
namespace Viox.Client.Podverse.Services;

using Microsoft.Extensions.Logging;

using Viox.Client.Podverse.Models;
using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Podverse implementation of <see cref="IMediaSource"/> registered under key "Podverse".
/// </summary>
public class PodverseMediaSource : IMediaSource
{
    private readonly ILogger<PodverseMediaSource> _logger;
    private readonly IPodverseClient _client;
    private readonly PodcastEpisodeParser _rssParser;
    private readonly MediaMetaDataConverterResolver _resolver;

    public string Source { get => "podverse"; }

    public PodverseMediaSource(
        IPodverseClient client,
        PodcastEpisodeParser rssParser,
        MediaMetaDataConverterResolver resolver,
        ILogger<PodverseMediaSource> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(rssParser);
        ArgumentNullException.ThrowIfNull(logger);

        _resolver = resolver;
        _rssParser = rssParser;
        _client = client;
        _logger = logger;
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct = default)
    {
        if (uri is null)
        {
            _logger.LogWarning("Unable to resolve metadata: MediaUri is null.");
            return null;
        }

        Podcast? podcast = await _client.GetPodcastByIdAsync(uri.Id, ct);
        if (podcast is null)
        {
            _logger.LogWarning("Unable to resolve metadata: Podverse status or track is null.");
            return null;
        }

        MediaUri mediaUri = uri;

        if (mediaUri.Type != "episode" && string.IsNullOrEmpty(mediaUri.SecondaryId))
        {
            return MapToMediaMetaData(podcast, mediaUri);
        }

        if (podcast.FeedUrls is not null && podcast.FeedUrls.Count > 0)
        {
            FeedUrl? firstPublicFeed = podcast.FeedUrls
                .OfType<FeedUrl>()
                .FirstOrDefault(f => f.IsPublic is true)
                ?? podcast.FeedUrls
                    .OfType<FeedUrl>()
                    .FirstOrDefault(f => !string.IsNullOrEmpty(f.Url));

            if (firstPublicFeed is not null && !string.IsNullOrEmpty(firstPublicFeed.Url))
            {
                IReadOnlyList<PodcastEpisode>? episodes = await _rssParser.ParseEpisodesFromUrlAsync(podcast.Id, firstPublicFeed.Url, ct);
                if (episodes is null)
                {
                    return null;
                }

                PodcastEpisode? episode = episodes
                    .OfType<PodcastEpisode>()
                    .FirstOrDefault(ep => ep.Guid is not null && ep.Guid == mediaUri.SecondaryId);

                if (episode is null)
                {
                    return null;
                }
                IMediaMetaDataConverterBase? converter = _resolver.ResolveConverter("podverse:episode");
                if (converter is not null)
                {
                    return converter.Convert(episode);
                }


            }
        }

        return null;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        PodversePagedResult<Podcast>? res = await _client.GetPodcastsPagedAsync(query, pageNumber, ct);
        if (res == null)
        {
            return new PagedList<MediaMetaData>();
        }

        List<MediaMetaData> items = res.Items?.OfType<Podcast>()
            .Select(podcast => MapToMediaMetaData(podcast))
            .ToList() ?? [];

        int offset = (pageNumber - 1) * limit;
        return new PagedList<MediaMetaData>(items, res.TotalCount, offset, items.Count());
    }

    private static MediaMetaData MapToMediaMetaData(Podcast podcast, MediaUri? customUri = null)
    {
        return new MediaMetaData
        {
            Uri = customUri ?? podcast.Uri?.ParseMediaUri(),
            Title = podcast.Title ?? string.Empty,
            Album = string.IsNullOrEmpty(podcast.Subtitle) ? string.Empty : podcast.Subtitle,
            Artist = FormatAuthors(podcast.Authors),
            Url = string.IsNullOrEmpty(podcast.LinkUrl) ? string.Empty : podcast.LinkUrl,
            ImageUrl = string.IsNullOrEmpty(podcast.ImageUrl) ? string.Empty : podcast.ImageUrl
        };
    }

    private static string FormatAuthors(IEnumerable<Author>? authors)
    {
        if (authors is null)
        {
            return string.Empty;
        }

        IEnumerable<string> authorNames = authors
            .OfType<Author>()
            .Where(a => !string.IsNullOrWhiteSpace(a.Name))
            .Select(a => a.Name!);

        return string.Join(", ", authorNames);
    }
}
