// File: MediaMetaData.cs
namespace Viox.Core.Models;

/// <summary>
/// Represents descriptive metadata for a resolved media resource.
/// </summary>
public record MediaMetaData
{
    /// <summary>
    /// Gets the structured <see cref="MediaUri"/> identifying the media item.
    /// </summary>
    public MediaUri? Uri { get; init; }

    /// <summary>
    /// Gets the display album of the media item.
    /// </summary>
    public required string Album { get; init; }

    /// <summary>
    /// Gets the display title of the media item.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the artist, creator, or broadcaster associated with the media item.
    /// </summary>
    public required string Artist { get; init; }

    /// <summary>
    /// Gets the primary playback or web location URL for the media item.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the web location URL of the thumbnail or album artwork image.
    /// </summary>
    public required string ImageUrl { get; init; }
}

