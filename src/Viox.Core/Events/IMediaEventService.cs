using Viox.Core.Models;

namespace Viox.Core.Events;

public interface IMediaEventService
{

    Task PublishMediaEventAsync(
        MediaEvent mediaEvent,
        CancellationToken ct);

    Task PublishPlaybackStatusAsync(
        PlaybackState state,
        CancellationToken ct);
}
