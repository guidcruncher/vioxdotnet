using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;

namespace Viox.Core.Services;

public sealed class ServerEventPublisher : IServerEventPublisher
{
    private readonly ConcurrentDictionary<Guid, Channel<EventPayload>> _subscribers = new();
    private readonly ILogger<ServerEventPublisher> _logger;
    private readonly ServerEventOptions _options;

    public ServerEventPublisher(
        ILogger<ServerEventPublisher> logger,
        IOptions<ServerEventOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public int ActiveSubscriberCount => _subscribers.Count;

    public async ValueTask PublishAsync(string eventType, string message, CancellationToken cancellationToken = default)
    {
        // 1. Short-circuit immediately if no clients are connected
        if (_subscribers.IsEmpty)
        {
            _logger.LogDebug("Skipped publishing event '{EventType}': No active subscribers connected.", eventType);
            return;
        }

        EventPayload payload = new(
            EventId: Guid.NewGuid().ToString("N"),
            EventType: eventType,
            Message: message,
            Timestamp: DateTime.UtcNow
        );

        _logger.LogInformation(
            "Broadcasting event '{EventType}' ({EventId}) to {Count} active subscriber(s).",
            payload.EventType,
            payload.EventId,
            _subscribers.Count);

        // 2. Broadcast payload to every connected client's personal channel
        foreach (KeyValuePair<Guid, Channel<EventPayload>> subscriber in _subscribers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!subscriber.Value.Writer.TryWrite(payload))
            {
                _logger.LogWarning("Subscriber {SubscriberId} queue full; dropping event {EventId}.", subscriber.Key, payload.EventId);
            }
        }

        await ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<EventPayload> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Guid subscriberId = Guid.NewGuid();

        BoundedChannelOptions channelOptions = new(_options.ChannelCapacity)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest
        };

        Channel<EventPayload> clientChannel = Channel.CreateBounded<EventPayload>(channelOptions);
        _subscribers.TryAdd(subscriberId, clientChannel);

        _logger.LogInformation("Client connected: {SubscriberId}. Total active subscribers: {Count}", subscriberId, _subscribers.Count);

        try
        {
            await foreach (EventPayload payload in clientChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return payload;
            }
        }
        finally
        {
            // 3. Automatic cleanup on client disconnect or cancellation
            _subscribers.TryRemove(subscriberId, out _);
            clientChannel.Writer.TryComplete();
            _logger.LogInformation("Client disconnected: {SubscriberId}. Remaining active subscribers: {Count}", subscriberId, _subscribers.Count);
        }
    }
}
