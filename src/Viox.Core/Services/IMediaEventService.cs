using Viox.Core.Models;

namespace Viox.Core.Services;

public interface IMediaEventService
{
    ValueTask PublishMediaActionAsync(string eventType, MediaMetaData? metadata, CancellationToken cancellationToken = default);
    ValueTask PublishPlaybackStatusAsync(PlaybackState state, CancellationToken cancellationToken = default);
}

