using Microsoft.Extensions.Logging;

using Viox.Core.Models;

namespace Viox.Core.Services;

public sealed class MediaEventService : IMediaEventService
{
    private readonly IServerEventPublisher _eventPublisher;
    private readonly ILogger<MediaEventService> _logger;

    public MediaEventService(
        IServerEventPublisher eventPublisher,
        ILogger<MediaEventService> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async ValueTask PublishMediaActionAsync(string eventType, MediaMetaData? metadata, CancellationToken cancellationToken = default)
    {
        if (metadata is not null)
        {
            _logger.LogInformation("Publishing media event '{EventType}' for track: {Title} by {Artist}", eventType, metadata.Title, metadata.Artist);
        }
        else
        {
            _logger.LogInformation("Publishing media event '{EventType}' with no active track metadata", eventType);
        }

        await _eventPublisher.PublishAsync(eventType, metadata, cancellationToken);
    }

    public async ValueTask PublishPlaybackStatusAsync(PlaybackState state, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing full playback status update.");

        await _eventPublisher.PublishAsync("status", state, cancellationToken);
    }
}
