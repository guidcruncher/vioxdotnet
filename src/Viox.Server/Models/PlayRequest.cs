namespace Viox.Server.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a request payload to play a specific media item.
/// </summary>
public sealed record PlayRequest
{
    /// <summary>
    /// Gets the target media URI to play.
    /// </summary>
    [Required]
    public required string Uri { get; init; }
}
