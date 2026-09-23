namespace Viox.Client.Mpd.Services;

/// <summary>
/// Defines the contract for an asynchronous Music Player Daemon (MPD) client.
/// </summary>
public interface IMpdClient
{
    /// <summary>
    /// Connects asynchronously to the MPD server using configured options.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a raw text command to the MPD server and reads the response.
    /// </summary>
    /// <param name="command">The command string to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The raw text response returned by the server.</returns>
    Task<string> SendCommandAsync(string command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts audio playback.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PlayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the current playlist, adds the specified file or stream URL, and begins playback immediately.
    /// </summary>
    /// <param name="fileOrUrl">The relative file path in the MPD library or a stream URL (e.g. HTTP stream).</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PlayFileOrUrlAsync(string fileOrUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all tracks from the active playlist (queue).
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearPlaylistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses audio playback.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops audio playback.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances playback to the next track in the queue.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns playback to the previous track in the queue.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PreviousAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the connection to the MPD server gracefully.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current playback position of the active track in seconds.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The current playback position in seconds, or <see langword="null"/> if stopped or unavailable.</returns>
    Task<double?> GetCurrentPositionAsync(CancellationToken cancellationToken = default);
}
