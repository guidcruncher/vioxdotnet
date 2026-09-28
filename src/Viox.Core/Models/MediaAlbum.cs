// File: MediaMetaData.cs
namespace Viox.Core.Models;

/// <summary>
/// Represents descriptive metadata for a resolved media resource.
/// </summary>
public class MediaAlbum : MediaMetaData
{

    public List<MediaMetaData> Tracks { get; set; } = new();
}

