// File: PlaylistIndexInitializationService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;

namespace Viox.Core.Playlists;

/// <summary>
/// Background worker responsible for auto-initializing the playlist index on startup if enabled.
/// </summary>
public class PlaylistIndexInitializationService : IHostedService
{
    private readonly IPlaylistIndexService _indexService;
    private readonly PlaylistOptions _options;
    private readonly ILogger<PlaylistIndexInitializationService> _logger;

    public PlaylistIndexInitializationService(
        IPlaylistIndexService indexService,
        IOptions<PlaylistOptions> options,
        ILogger<PlaylistIndexInitializationService> logger)
    {
        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.AutoInitializeOnStartup)
        {
            _logger.LogInformation("Auto-initializing playlist index on startup...");
            await _indexService.RebuildIndexAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("Auto-initialize on startup is disabled in configuration.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
