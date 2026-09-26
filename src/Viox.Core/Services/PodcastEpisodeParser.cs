using System.Globalization;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;

namespace Viox.Core.Services;

/// <summary>
/// Service responsible for mapping RSS stream channel items into PodcastEpisode models.
/// </summary>
public class PodcastEpisodeParser
{
    private static readonly XNamespace ItunesNs = "http://www.itunes.com/dtds/podcast-1.0.dtd";

    private static readonly string[] RssDateFormats =
    [
        "ddd, dd MMM yyyy HH:mm:ss zzz",
        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
        "ddd, dd MMM yyyy HH:mm:ss 'UTC'",
        "dd MMM yyyy HH:mm:ss zzz",
        "dd MMM yyyy HH:mm:ss 'GMT'",
        "dd MMM yyyy HH:mm:ss 'UTC'",
        "ddd, dd MMM yyyy HH:mm zzz",
        "dd MMM yyyy HH:mm zzz"
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PodcastEpisodeParser> _logger;
    private readonly PodcastEpisodeParserOptions _options;
    private readonly IHtmlSanitizerService _sanitizer;

    public PodcastEpisodeParser(
        IHttpClientFactory httpClientFactory,
        ILogger<PodcastEpisodeParser> logger,
        IHtmlSanitizerService sanitizer,
        IOptions<PodcastEpisodeParserOptions> options)
    {
        _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Downloads an RSS feed from a URL string and parses its items into PodcastEpisode records.
    /// </summary>
    public async Task<IReadOnlyList<PodcastEpisode>> ParseEpisodesFromUrlAsync(string id, string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Provided URL must be a valid absolute URI.", nameof(url));
        }

        MediaUri? uriParsed = id.ParseMediaUri();

        return await ParseEpisodesFromUrlAsync(uriParsed is null ? id : uriParsed.Id, uri, cancellationToken);
    }

    /// <summary>
    /// Downloads an RSS feed from a Uri and parses its items into PodcastEpisode records.
    /// </summary>
    public async Task<IReadOnlyList<PodcastEpisode>> ParseEpisodesFromUrlAsync(string id, Uri requestUri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        _logger.LogInformation("Downloading podcast RSS feed from URL: {Url}", requestUri);

        var client = _httpClientFactory.CreateClient(nameof(PodcastEpisodeParser));
        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var xmlStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await ParseEpisodesAsync(id, xmlStream, cancellationToken);
    }

    /// <summary>
    /// Reads an RSS feed stream and returns a populated collection of PodcastEpisode records.
    /// </summary>
    public async Task<IReadOnlyList<PodcastEpisode>> ParseEpisodesAsync(string id, Stream xmlStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(xmlStream);

        _logger.LogInformation("Starting extraction of podcast episodes from RSS XML feed.");

        var document = await XDocument.LoadAsync(xmlStream, LoadOptions.None, cancellationToken);
        var channel = document.Root?.Element("channel");

        if (channel == null)
        {
            _logger.LogError("Failed to parse RSS feed: Missing 'channel' root element.");
            throw new InvalidDataException("Invalid RSS XML feed: Channel element missing.");
        }

        var channelImageUrl = channel.Element(ItunesNs + "image")?.Attribute("href")?.Value
                             ?? channel.Element("image")?.Element("url")?.Value;

        var episodes = new List<PodcastEpisode>();

        foreach (var item in channel.Elements("item"))
        {
            var enclosure = item.Element("enclosure");
            var audioUrl = enclosure?.Attribute("url")?.Value ?? string.Empty;
            var audioMimeType = enclosure?.Attribute("type")?.Value;

            long? audioSizeBytes = null;
            if (long.TryParse(enclosure?.Attribute("length")?.Value, out var parsedSize))
            {
                audioSizeBytes = parsedSize;
            }

            DateTimeOffset? publishedDate = null;
            string? cleanDate = item.Element("pubDate")?.Value;
            if (!string.IsNullOrEmpty(cleanDate))
            {
                cleanDate = cleanDate.Trim();

                if (DateTimeOffset.TryParseExact(
                        cleanDate,
                        RssDateFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                        out DateTimeOffset parsedDate))
                {
                    publishedDate = parsedDate;
                }
            }

            var description = item.Element("description")?.Value;
            if (string.IsNullOrWhiteSpace(description) && _options.FallbackToItunesSummary)
            {
                description = item.Element(ItunesNs + "summary")?.Value;
            }

            var episodeImageUrl = item.Element(ItunesNs + "image")?.Attribute("href")?.Value
                                 ?? item.Element("image")?.Element("url")?.Value
                                 ?? channelImageUrl;

            var title = item.Element("title")?.Value
                        ?? item.Element(ItunesNs + "title")?.Value
                        ?? string.Empty;

            if (!RssDurationParser.TryParseToSeconds(item.Element(ItunesNs + "duration")?.Value ?? string.Empty, out double durationSeconds))
            {
                durationSeconds = 0;
            }

            var episode = new PodcastEpisode
            {
                Uri = $"podverse:episode:{id}:{item.Element("guid")?.Value}",
                PodcastId = id,
                Title = title,
                Description = string.IsNullOrEmpty(description) ? string.Empty : _sanitizer.StripHtml(description),
                PublishedDate = publishedDate,
                AudioUrl = audioUrl,
                AudioMimeType = audioMimeType,
                AudioSizeBytes = audioSizeBytes,
                Duration = item.Element(ItunesNs + "duration")?.Value,
                DurationSeconds = durationSeconds,
                Guid = item.Element("guid")?.Value,
                Link = item.Element("link")?.Value,
                ImageUrl = episodeImageUrl
            };

            episodes.Add(episode);
        }

        _logger.LogInformation("Successfully parsed {Count} podcast episodes.", episodes.Count);

        return episodes;
    }
}
