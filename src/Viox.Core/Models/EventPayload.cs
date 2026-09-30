namespace Viox.Core.Models;

public sealed record EventPayload(
    string EventId,
    string EventType,
    string Message,
    DateTime Timestamp
);

