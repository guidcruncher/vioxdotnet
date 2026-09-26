namespace Viox.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Podverse.Models;
using Viox.Client.Podverse.Services;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Server.Configuration;

/// <summary>
/// Background service that executes daily at a specified scheduled time to download podcasts using <see cref="AudioCacheManager"/>.
/// </summary>
public sealed class PodcastDownloaderBackgroundService : BackgroundService
{
    private readonly IPodverseClient _podverseClient;
    private readonly PodcastEpisodeParser _rssParser;
    private readonly MediaMetaDataConverterResolver _resolver;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<PodcastDownloadOptions> _optionsMonitor;
    private readonly ILogger<PodcastDownloaderBackgroundService> _logger;

    public PodcastDownloaderBackgroundService(
        IPodverseClient podverseClient,
        MediaMetaDataConverterResolver resolver,
        PodcastEpisodeParser rssParser,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<PodcastDownloadOptions> optionsMonitor,
        ILogger<PodcastDownloaderBackgroundService> logger)
    {
        _podverseClient = podverseClient ?? throw new ArgumentNullException(nameof(podverseClient));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _rssParser = rssParser ?? throw new ArgumentNullException(nameof(rssParser));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task<List<MediaMetaData>> GetPodcastEpisodesAsync(string podcastId, CancellationToken cancellationToken)
    {
        List<MediaMetaData> items = new();
        _logger.LogInformation("Fetching podcast by ID: {PodcastId}", podcastId);

        IMediaMetaDataConverterBase? converter = _resolver.ResolveConverter("podverse:episode");
        if (converter is null)
        {
            return items;
        }

        var podcast = await _podverseClient.GetPodcastByIdAsync(podcastId, cancellationToken);
        if (podcast is null)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", podcastId);
            return items;
        }

        if (podcast.FeedUrls?.Count > 0)
        {
            FeedUrl? firstPublicFeed = podcast.FeedUrls.FirstOrDefault(f => f.IsPublic is true)
                ?? podcast.FeedUrls.FirstOrDefault(f => !string.IsNullOrEmpty(f.Url));

            if (firstPublicFeed != null)
            {
                IReadOnlyList<PodcastEpisode> episodes = await _rssParser.ParseEpisodesFromUrlAsync(podcast.Id, firstPublicFeed.Url, cancellationToken);
                foreach (PodcastEpisode episode in episodes)
                {
                    MediaMetaData? item = converter.Convert(episode);
                    if (item is not null)
                    {
                        if (string.IsNullOrEmpty(item.ImageUrl))
                        {
                            item.ImageUrl = !string.IsNullOrEmpty(podcast.ImageUrl) ? podcast.ImageUrl : string.Empty;
                        }
                        items.Add(item);
                    }
                }
            }
        }

        return items;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Podcast Downloader Background Service starting.");
        if (_optionsMonitor.CurrentValue.RunOnStartup)
        {
            await ProcessBatchSafelyAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = CalculateDelayToNextExecution(_optionsMonitor.CurrentValue.DailyScheduleTime);
            _logger.LogInformation(
                "Next podcast download scheduled for daily run at {TargetTime} (Waiting for {Delay})",
                _optionsMonitor.CurrentValue.DailyScheduleTime,
                delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await ProcessBatchSafelyAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Podcast Downloader Background Service stopping.");
    }

    private static TimeSpan CalculateDelayToNextExecution(TimeOnly targetTime)
    {
        DateTime now = DateTime.Now;
        DateTime nextRun = now.Date.Add(targetTime.ToTimeSpan());
        if (now >= nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }
        return nextRun - now;
    }

    private async Task ProcessBatchSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var podcastProvider = scope.ServiceProvider.GetRequiredService<IPodcastProvider>();
            var cacheManager = scope.ServiceProvider.GetRequiredService<AudioCacheManager>();

            double hoursToSubtract = Math.Abs(_optionsMonitor.CurrentValue.MaxAgeHours);
            DateTimeOffset maxAge = DateTimeOffset.UtcNow.AddHours(-hoursToSubtract);

            IEnumerable<MediaMetaData> metadataList = await podcastProvider.GetPodcastsToDownloadAsync(cancellationToken);
            List<MediaMetaData> itemsToProcess = metadataList?.ToList() ?? new List<MediaMetaData>();

            if (itemsToProcess.Count == 0)
            {
                _logger.LogDebug("No podcast media metadata found to download during this execution cycle.");
                return;
            }

            _logger.LogInformation("Starting download process for {Count} podcast items.", itemsToProcess.Count);
            _logger.LogInformation("Podcast download cutoff date {maxAge} ", maxAge);
            int maxParallelism = Math.Max(1, _optionsMonitor.CurrentValue.MaxDegreeOfParallelism);
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(itemsToProcess, parallelOptions, async (podcast, ct) =>
            {
                string? podcastId = podcast.Uri?.Id;
                if (string.IsNullOrEmpty(podcastId))
                {
                    _logger.LogWarning("Skipping podcast item with missing or null URI ID.");
                    return;
                }

                List<MediaMetaData> items = await GetPodcastEpisodesAsync(podcastId, ct);

                foreach (MediaMetaData item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.Url))
                    {
                        _logger.LogWarning("Skipping media title '{Title}' because URL is null or empty.", item.Title);
                        continue;
                    }

                    _logger.LogDebug("Processing podcast download for Title: '{Title}' from URL: {Url}", item.Title, item.Url);
                    string? resultPath = await cacheManager.GetOrDownloadAudioAsync(item.Url, item.ReleaseDate, ct);

                    if (item.ReleaseDate is not null && item.ReleaseDate < maxAge)
                    {
                        // _logger.LogWarning("Skipping media title '{Title}' because It is too old past {maxAge}.", item.Title, maxAge);
                        continue;
                    }

                    if (resultPath is null)
                    {
                        _logger.LogWarning("Failed to cache title '{Title}' from URL: {Url}", item.Title, item.Url);
                    }
                }
            });

            _logger.LogInformation("Completed download cycle for {Count} podcast items.", itemsToProcess.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Podcast download cycle was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred during scheduled podcast download cycle.");
        }
    }
}
