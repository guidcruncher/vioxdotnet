namespace Viox.Core.Models;

public sealed record EmitEventRequest(
    string EventType,
    string Message
);
