using Viox.Core.Models;

namespace Viox.Core.Events;

public sealed record MediaEvent(
    MediaEventType Type,
    string Backend,
    MediaMetaData? Track,
    object? Data = null,
    DateTimeOffset? Timestamp = null);
