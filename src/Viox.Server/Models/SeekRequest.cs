namespace Viox.Server.Models;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a request payload to seek to a position in active media.
/// </summary>
public sealed record SeekRequest
{
    /// <summary>
    /// Gets the seek position offset.
    /// </summary>
    [Required]
    public required TimeSpan Position { get; init; }
}
