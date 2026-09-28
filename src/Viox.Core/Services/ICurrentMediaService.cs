namespace Viox.Core.Services;

using System;

using Viox.Core.Models;

/// <summary>
/// Event arguments supplied when the currently playing media metadata changes.
/// </summary>
public sealed class MediaChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current media metadata, or <see langword="null"/> if playback stopped.
    /// </summary>
    public MediaMetaData? CurrentMedia { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaChangedEventArgs"/> class.
    /// </summary>
    /// <param name="currentMedia">The updated media metadata.</param>
    public MediaChangedEventArgs(MediaMetaData? currentMedia)
    {
        CurrentMedia = currentMedia;
    }
}

/// <summary>
/// Defines a singleton service for persisting and observing globally playing media state.
/// </summary>
public interface ICurrentMediaService
{
    /// <summary>
    /// Occurs when the currently playing media changes.
    /// </summary>
    event EventHandler<MediaChangedEventArgs>? CurrentMediaChanged;

    /// <summary>
    /// Gets the currently active media metadata, or <see langword="null"/> if no media is playing.
    /// </summary>
    MediaMetaData? CurrentMedia { get; }

    /// <summary>
    /// Updates the active global media metadata and notifies subscribers.
    /// </summary>
    /// <param name="mediaMetaData">The updated media metadata, or <see langword="null"/> to clear state.</param>
    void SetCurrentMedia(MediaMetaData? mediaMetaData);

    /// <summary>
    /// Clears the currently playing media state.
    /// </summary>
    void ClearCurrentMedia();
}

