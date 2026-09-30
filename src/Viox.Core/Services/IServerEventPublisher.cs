using Viox.Core.Models;

namespace Viox.Core.Services;

public interface IServerEventPublisher
{
    int ActiveSubscriberCount { get; }
    ValueTask PublishAsync(string eventType, string message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<EventPayload> SubscribeAsync(CancellationToken cancellationToken);
}
