// File: MediaUri.cs
namespace Viox.Core.Models;

/// <summary>
/// Represents a parsed media identifier containing source, classification type, and primary/secondary IDs.
/// </summary>
public record MediaUri
{
    /// <summary>
    /// Gets the provider or platform source of the media item (e.g., spotify, tunein).
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// Gets the classification type of the media item (e.g., album, track, podcast).
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets the primary unique identifier for the media resource.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets the optional secondary identifier associated with the media resource.
    /// </summary>
    public string? SecondaryId { get; set; }

    public override string ToString()
    {
        return $"{Source}:{Type}:{Id}" + (string.IsNullOrEmpty(SecondaryId) ? string.Empty : $":{SecondaryId}");
    }
}
