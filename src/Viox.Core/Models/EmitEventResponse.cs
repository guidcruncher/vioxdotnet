namespace Viox.Core.Models;

public sealed record EmitEventResponse(
    string Status,
    int ActiveSubscribers
);
