namespace Viox.Server.Models;

/// <summary>
/// Data transfer object for requesting playback of a specific file path or streaming URL.
/// </summary>
public sealed record MpdPlayRequest
{
    /// <summary>
    /// Gets the relative media path within the MPD music directory or a valid HTTP stream URL.
    /// </summary>
    public required string FileOrUrl { get; init; }
}
