namespace Viox.Server.Abstraction;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Common contract for underlying media engine adapters.
/// </summary>
public interface IMediaPlayerAdapter
{

    /// <summary>
    /// Use Proxy if true
    /// </Summary>
    bool UseProxy { get; }

    /// <summary>
    /// Gets the unique identifier name for the engine (e.g., "Librespot", "MPV").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Determines whether this engine adapter can handle the given media URI.
    /// </summary>
    bool CanHandle(string uri);

    /// <summary>
    /// Asynchronously checks if this engine is currently playing active media.
    /// </summary>
    Task<bool> IsActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts or loads playback for the specified URI.
    /// </summary>
    Task PlayAsync(string uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses active playback.
    /// </summary>
    Task PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes paused playback.
    /// </summary>
    Task ResumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops playback completely.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeks to a specific position in the media timeline.
    /// </summary>
    Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigates to the next track or item in queue.
    /// </summary>
    Task NextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigates to the previous track or item in queue.
    /// </summary>
    Task PreviousAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the output volume percentage (0 to 100).
    /// </summary>
    Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves current volume level percentage.
    /// </summary>
    Task<int> GetVolumeAsync(CancellationToken cancellationToken = default);

    Task<double> GetPlaybackPositionAsync(CancellationToken cancellationToken = default);
}
