namespace Viox.Server.Models;

/// <summary>
/// Specifies the repeat behavior for queue playback execution.
/// </summary>
public enum PlaybackRepeatMode
{
    /// <summary>
    /// Playback stops when reaching the end of the queue.
    /// </summary>
    Off = 0,

    /// <summary>
    /// Repeats the currently active track indefinitely.
    /// </summary>
    One = 1,

    /// <summary>
    /// Loops back to the start of the queue after reaching the end.
    /// </summary>
    All = 2
}
