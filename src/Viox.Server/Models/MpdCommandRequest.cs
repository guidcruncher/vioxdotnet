namespace Viox.Server.Models;

/// <summary>
/// Data transfer object for sending arbitrary text commands directly to the MPD server.
/// </summary>
public sealed record MpdCommandRequest
{
    /// <summary>
    /// Gets the command string to execute (e.g., "status", "currentsong").
    /// </summary>
    public required string Command { get; init; }
}
