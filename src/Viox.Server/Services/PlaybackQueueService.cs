namespace Viox.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Models;
using Viox.Server.Abstraction;
using Viox.Server.Configuration;
using Viox.Server.Models;

/// <summary>
/// Thread-safe playback queue manager storing MediaMetaData instances and raising decoupled playback events.
/// </summary>
public sealed class PlaybackQueueService : IPlaybackQueueService
{
    private readonly IOptions<PlaybackQueueOptions> _options;
    private readonly ILogger<PlaybackQueueService> _logger;
    private readonly Lock _lock = new();

    private readonly List<MediaMetaData> _items = [];
    private List<int> _playOrder = [];
    private int _orderIndex = -1;

    public event Func<MediaMetaData, CancellationToken, Task>? TrackPlayRequested;

    public event Func<CancellationToken, Task>? PlaybackStopRequested;

    public PlaybackQueueService(
        IOptions<PlaybackQueueOptions> options,
        ILogger<PlaybackQueueService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<MediaMetaData> Queue
    {
        get
        {
            lock (_lock)
            {
                return _items.ToList().AsReadOnly();
            }
        }
    }

    public int CurrentIndex
    {
        get
        {
            lock (_lock)
            {
                if (_orderIndex < 0 || _orderIndex >= _playOrder.Count)
                {
                    return -1;
                }
                return _playOrder[_orderIndex];
            }
        }
    }

    public MediaMetaData? CurrentItem
    {
        get
        {
            lock (_lock)
            {
                int index = CurrentIndex;
                return index >= 0 && index < _items.Count ? _items[index] : null;
            }
        }
    }

    public bool IsShuffleEnabled { get; private set; }

    public PlaybackRepeatMode RepeatMode { get; private set; } = PlaybackRepeatMode.Off;

    public async Task EnqueueAsync(MediaMetaData item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        MediaMetaData? playTarget = null;

        lock (_lock)
        {
            if (_items.Count >= _options.Value.MaxQueueSize)
            {
                _logger.LogWarning("Queue length limit reached ({Limit}). Track '{Title}' by '{Artist}' rejected.", _options.Value.MaxQueueSize, item.Title, item.Artist);
                return;
            }

            _items.Add(item);
            int newIndex = _items.Count - 1;
            _playOrder.Add(newIndex);

            _logger.LogInformation("Enqueued '{Title}' by '{Artist}' (Album: '{Album}', RawUri: '{RawUri}'). Total count: {Count}.", item.Title, item.Artist, item.Album, item.RawUri, _items.Count);

            if (_options.Value.AutoPlayOnEnqueue && _orderIndex < 0)
            {
                _orderIndex = 0;
                playTarget = _items[_playOrder[_orderIndex]];
            }
        }

        if (playTarget is not null)
        {
            await ExecutePlayAsync(playTarget, cancellationToken);
        }
    }

    public async Task EnqueueRangeAsync(IEnumerable<MediaMetaData> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (MediaMetaData item in items)
        {
            await EnqueueAsync(item, cancellationToken);
        }
    }

    public async Task<bool> PlayNextAsync(CancellationToken cancellationToken = default)
    {
        MediaMetaData? playTarget = null;

        lock (_lock)
        {
            if (_items.Count == 0)
            {
                return false;
            }

            if (RepeatMode == PlaybackRepeatMode.One && _orderIndex >= 0)
            {
                playTarget = _items[_playOrder[_orderIndex]];
            }
            else if (_orderIndex + 1 < _playOrder.Count)
            {
                _orderIndex++;
                playTarget = _items[_playOrder[_orderIndex]];
            }
            else if (RepeatMode == PlaybackRepeatMode.All)
            {
                _orderIndex = 0;
                playTarget = _items[_playOrder[_orderIndex]];
            }
            else
            {
                _logger.LogInformation("Queue completed. No further tracks available.");
                return false;
            }
        }

        if (playTarget is not null)
        {
            await ExecutePlayAsync(playTarget, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task<bool> PlayPreviousAsync(CancellationToken cancellationToken = default)
    {
        MediaMetaData? playTarget = null;

        lock (_lock)
        {
            if (_items.Count == 0)
            {
                return false;
            }

            if (RepeatMode == PlaybackRepeatMode.One && _orderIndex >= 0)
            {
                playTarget = _items[_playOrder[_orderIndex]];
            }
            else if (_orderIndex - 1 >= 0)
            {
                _orderIndex--;
                playTarget = _items[_playOrder[_orderIndex]];
            }
            else if (RepeatMode == PlaybackRepeatMode.All)
            {
                _orderIndex = _playOrder.Count - 1;
                playTarget = _items[_playOrder[_orderIndex]];
            }
            else
            {
                return false;
            }
        }

        if (playTarget is not null)
        {
            await ExecutePlayAsync(playTarget, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task<bool> SkipToAsync(int index, CancellationToken cancellationToken = default)
    {
        MediaMetaData? playTarget = null;

        lock (_lock)
        {
            if (index < 0 || index >= _items.Count)
            {
                _logger.LogWarning("Requested skip index {Index} is out of bounds.", index);
                return false;
            }

            int orderPos = _playOrder.IndexOf(index);
            if (orderPos >= 0)
            {
                _orderIndex = orderPos;
                playTarget = _items[index];
            }
        }

        if (playTarget is not null)
        {
            await ExecutePlayAsync(playTarget, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task RemoveAtAsync(int index, CancellationToken cancellationToken = default)
    {
        MediaMetaData? nextPlayTarget = null;
        bool shouldStop = false;

        lock (_lock)
        {
            if (index < 0 || index >= _items.Count)
            {
                return;
            }

            bool removingCurrent = CurrentIndex == index;

            _items.RemoveAt(index);
            RebuildPlayOrder(preserveCurrentIndex: true);

            if (removingCurrent && _items.Count > 0)
            {
                if (_orderIndex >= _playOrder.Count)
                {
                    _orderIndex = _playOrder.Count - 1;
                }

                if (_orderIndex >= 0)
                {
                    nextPlayTarget = _items[_playOrder[_orderIndex]];
                }
            }
            else if (_items.Count == 0)
            {
                _orderIndex = -1;
                shouldStop = true;
            }
        }

        if (nextPlayTarget is not null)
        {
            await ExecutePlayAsync(nextPlayTarget, cancellationToken);
        }
        else if (shouldStop)
        {
            await ExecuteStopAsync(cancellationToken);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _items.Clear();
            _playOrder.Clear();
            _orderIndex = -1;
        }

        _logger.LogInformation("Playback queue cleared.");
        await ExecuteStopAsync(cancellationToken);
    }

    public Task SetShuffleAsync(bool enable, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (IsShuffleEnabled == enable)
            {
                return Task.CompletedTask;
            }

            IsShuffleEnabled = enable;
            RebuildPlayOrder(preserveCurrentIndex: true);
            _logger.LogInformation("Shuffle state set to {Status}.", IsShuffleEnabled);
        }

        return Task.CompletedTask;
    }

    public Task SetRepeatModeAsync(PlaybackRepeatMode repeatMode, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            RepeatMode = repeatMode;
            _logger.LogInformation("Repeat mode changed to {Mode}.", RepeatMode);
        }

        return Task.CompletedTask;
    }

    private void RebuildPlayOrder(bool preserveCurrentIndex)
    {
        int currentTrackIndex = CurrentIndex;

        List<int> newOrder = Enumerable.Range(0, _items.Count).ToList();

        if (IsShuffleEnabled && newOrder.Count > 1)
        {
            Random random = Random.Shared;
            for (int i = newOrder.Count - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                (newOrder[i], newOrder[k]) = (newOrder[k], newOrder[i]);
            }
        }

        _playOrder = newOrder;

        if (preserveCurrentIndex && currentTrackIndex >= 0 && currentTrackIndex < _items.Count)
        {
            _orderIndex = _playOrder.IndexOf(currentTrackIndex);
        }
        else
        {
            _orderIndex = _playOrder.Count > 0 ? 0 : -1;
        }
    }

    private async Task ExecutePlayAsync(MediaMetaData item, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing TrackPlayRequested event for track '{Title}' by '{Artist}'.", item.Title, item.Artist);
        if (TrackPlayRequested is not null)
        {
            await TrackPlayRequested.Invoke(item, cancellationToken);
        }
    }

    private async Task ExecuteStopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing PlaybackStopRequested event.");
        if (PlaybackStopRequested is not null)
        {
            await PlaybackStopRequested.Invoke(cancellationToken);
        }
    }
}
