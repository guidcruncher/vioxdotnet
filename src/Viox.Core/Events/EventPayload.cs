namespace Viox.Core.Events;

public sealed record EventPayload(
    string EventId,
    string EventType,
    string Message,
    DateTime Timestamp
);
