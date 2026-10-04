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

    /// <summary>
    /// Gets or sets a value indicating whether the index should automatically populate on startup.
    /// </summary>
    public bool AutoInitializeOnStartup { get; set; } = true;
}
