// AudioCacheCleanupService.cs
namespace Viox.Server.Services;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Server.Configuration;

public class AudioCacheCleanupService : BackgroundService
{
    private readonly AudioCacheManager _cacheManager;
    private readonly ILogger<AudioCacheCleanupService> _logger;
    private readonly IOptionsMonitor<PodcastDownloadOptions> _optionsMonitor;

    public AudioCacheCleanupService(
        AudioCacheManager cacheManager,
        IOptionsMonitor<PodcastDownloadOptions> optionsMonitor,
        ILogger<AudioCacheCleanupService> logger)
    {
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string cacheDir = _cacheManager.GetCacheDirectory();
                if (Directory.Exists(cacheDir))
                {
                    var files = Directory.GetFiles(cacheDir, "*.mp3");
                    double hoursToSubtract = Math.Abs(_optionsMonitor.CurrentValue.MaxAgeHours);
                    DateTime cutoff = DateTime.UtcNow.AddHours(-hoursToSubtract);

                    foreach (var file in files)
                    {
                        var info = new FileInfo(file);
                        if (info.LastAccessTimeUtc < cutoff)
                        {
                            try
                            {
                                info.Delete();
                                _logger.LogInformation("Evicted old audio file from local cache: {FilePath}", file);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete cached file: {FilePath}", file);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during cache eviction scan.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
