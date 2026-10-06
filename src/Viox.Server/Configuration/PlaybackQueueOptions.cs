namespace Viox.Server.Configuration;

/// <summary>
/// Configuration options governing queue management and auto-advance settings.
/// </summary>
public sealed class PlaybackQueueOptions
{
    public const string SectionName = "PlaybackQueue";

    /// <summary>
    /// Gets or sets whether enqueuing into an empty queue triggers immediate playback.
    /// </summary>
    public bool AutoPlayOnEnqueue { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum allowable items in the queue.
    /// </summary>
    public int MaxQueueSize { get; set; } = 1000;
}
