namespace Viox.Core.Events;

public interface IServerEventPublisher
{
    int ActiveSubscriberCount { get; }
    ValueTask PublishAsync(string eventType, string message, CancellationToken cancellationToken = default);
    ValueTask PublishAsync(MediaEvent mediaEvent, CancellationToken cancellaionToken = default);
    IAsyncEnumerable<EventPayload> SubscribeAsync(CancellationToken cancellationToken);
}
