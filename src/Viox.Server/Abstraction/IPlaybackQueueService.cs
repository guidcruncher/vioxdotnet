namespace Viox.Server.Abstraction;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;
using Viox.Server.Models;

/// <summary>
/// Abstraction for managing media playback queue state and emitting playback navigation events.
/// </summary>
public interface IPlaybackQueueService
{
    /// <summary>
    /// Raised when a track in the queue requests execution by the playback engine.
    /// </summary>
    event Func<MediaMetaData, CancellationToken, Task>? TrackPlayRequested;

    /// <summary>
    /// Raised when playback should be stopped due to queue clearance or exhaustion.
    /// </summary>
    event Func<CancellationToken, Task>? PlaybackStopRequested;

    IReadOnlyList<MediaMetaData> Queue { get; }

    int CurrentIndex { get; }

    MediaMetaData? CurrentItem { get; }

    bool IsShuffleEnabled { get; }

    PlaybackRepeatMode RepeatMode { get; }

    Task EnqueueAsync(MediaMetaData item, CancellationToken cancellationToken = default);

    Task EnqueueRangeAsync(IEnumerable<MediaMetaData> items, CancellationToken cancellationToken = default);

    Task<bool> PlayNextAsync(CancellationToken cancellationToken = default);

    Task<bool> PlayPreviousAsync(CancellationToken cancellationToken = default);

    Task<bool> SkipToAsync(int index, CancellationToken cancellationToken = default);

    Task RemoveAtAsync(int index, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);

    Task SetShuffleAsync(bool enable, CancellationToken cancellationToken = default);

    Task SetRepeatModeAsync(PlaybackRepeatMode repeatMode, CancellationToken cancellationToken = default);
}

