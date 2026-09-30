namespace Viox.Server.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Server.Abstraction;
using Viox.Server.Configuration;

/// <summary>
/// Background hosted service that periodically polls the media player control surface for status updates.
/// </summary>
public sealed class MediaPlayerPollingService : BackgroundService
{
    private readonly IMediaPlayerControlSurface _controlSurface;
    private readonly IOptionsMonitor<MediaPlayerOptions> _options;
    private readonly ILogger<MediaPlayerPollingService> _logger;

    public MediaPlayerPollingService(
        IMediaPlayerControlSurface controlSurface,
        IOptionsMonitor<MediaPlayerOptions> options,
        ILogger<MediaPlayerPollingService> logger)
    {
        _controlSurface = controlSurface ?? throw new ArgumentNullException(nameof(controlSurface));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Media Player Polling Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int intervalSeconds = _options.CurrentValue.PollingIntervalSeconds;
            if (intervalSeconds <= 0)
            {
                intervalSeconds = 5;
            }

            try
            {
                _logger.LogDebug("Polling media player status...");
                await _controlSurface.GetStatusAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown sequence, swallow exception and break loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while polling media player status.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Media Player Polling Service is stopping.");
    }
}
