namespace Viox.Server.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a request payload to change player volume.
/// </summary>
public sealed record SetVolumeRequest
{
    /// <summary>
    /// Gets the target volume percentage between 0 and 100.
    /// </summary>
    [Range(0, 100)]
    public required int VolumePercent { get; init; }

    /// <summary>
    /// Set mute state
    /// </summary>
    public bool Muted { get; init; }
}
