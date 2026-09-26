namespace Viox.Server.Configuration;

/// <summary>
/// Options for configuring default behavior on the unified control surface.
/// </summary>
public sealed class MediaPlayerOptions
{
    public const string SectionName = "MediaPlayer";

    /// <summary>
    /// Gets or sets the name of the default player engine to fallback to when no active player is resolved.
    /// </summary>
    public string DefaultPlayerName { get; set; } = "MPV";
}
