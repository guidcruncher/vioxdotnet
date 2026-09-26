// File: MediaMetaData.cs
namespace Viox.Core.Models;

/// <summary>
/// Represents descriptive metadata for a resolved media resource.
/// </summary>
public class MediaMetaData
{
    /// <summary>
    /// Gets the structured <see cref="MediaUri"/> identifying the media item.
    /// </summary>
    public MediaUri? Uri { get; set; }

    /// <summary>
    /// Gets the Raw URI value
    /// </summary>
    public string RawUri { get => (Uri is null ? string.Empty : (Uri.SecondaryId is null ? $"{Uri.Source}:{Uri.Type}:{Uri.Id}" : $"{Uri.Source}:{Uri.Type}:{Uri.Id}:{Uri.SecondaryId}")); }

    /// <summary>
    /// Gets the display album of the media item.
    /// </summary>
    public required string Album { get; set; }

    /// <summary>
    /// Gets the display title of the media item.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets the artist, creator, or broadcaster associated with the media item.
    /// </summary>
    public required string Artist { get; set; }

    /// <summary>
    /// Gets the primary playback or web location URL for the media item.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets the web location URL of the thumbnail or album artwork image.
    /// </summary>
    public required string ImageUrl { get; set; }

    /// <summary>
    /// Gets the playbach duration in seconds
    /// </summary>
    public double? Duration { get; set; }

    public bool Favourite { get; set; } = false;

    public DateTimeOffset? ReleaseDate { get; set; } = null;

}

