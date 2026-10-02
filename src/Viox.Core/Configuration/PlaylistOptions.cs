// File: PlaylistOptions.cs
namespace Viox.Core.Configuration;

/// <summary>
/// Configuration options for playlist storage and persistence.
/// </summary>
public class PlaylistOptions
{

    public const string SectionName = "PlaylistOptions";

    /// <summary>
    /// Gets or sets the directory path where playlists are stored.
    /// </summary>
    public string StorageDirectory { get; set; } = "/data/playlists";
}
