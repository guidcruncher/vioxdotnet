using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Core.Events;

public sealed class MediaEventService : IMediaEventService
{
    private readonly IServerEventPublisher _eventPublisher;
    private readonly ILogger<MediaEventService> _logger;
    private readonly ICurrentMediaService _currentMediaService;

    public MediaEventService(
        IServerEventPublisher eventPublisher,
        ICurrentMediaService currentMediaService,
        ILogger<MediaEventService> logger)
    {
        _eventPublisher = eventPublisher;
        _currentMediaService = currentMediaService;
        _logger = logger;
    }

    public async Task PublishMediaEventAsync(
        MediaEvent mediaEvent,
        CancellationToken cancellationToken = default)
    {
        await _eventPublisher.PublishAsync(mediaEvent, cancellationToken);
    }

    public async Task PublishPlaybackStatusAsync(PlaybackState state, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Publishing full playback status update.");

        PlaybackStateData data = new(
               state.Position,
               state.Playing,
          state.IsLive
          );

        MediaEvent mediaEvent = new(MediaEventType.Status, state.ActiveBackend,
            _currentMediaService.CurrentMedia,
            data,
            DateTimeOffset.UtcNow
        );

        await _eventPublisher.PublishAsync(mediaEvent, cancellationToken);
    }
}

public record PlaybackStateData(
double Position,
bool Playing,
bool IsLive);
