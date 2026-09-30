using Viox.Core.Models;

namespace Viox.Core.Services;

public interface IServerEventPublisher
{
    int ActiveSubscriberCount { get; }
    ValueTask PublishAsync(string eventType, string message, CancellationToken cancellationToken = default);
    ValueTask PublishAsync<T>(string EventType, T payload, CancellationToken cancellaionToken = default);
    IAsyncEnumerable<EventPayload> SubscribeAsync(CancellationToken cancellationToken);
}
