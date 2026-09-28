namespace Viox.Server.Models;

/// <summary>
/// Represents a single equalizer frequency band and its current state.
/// </summary>
public sealed record EqualizerBand(
    int Index,
    string ControlName,
    string FrequencyLabel,
    int LeftPercentage,
    int RightPercentage
);
