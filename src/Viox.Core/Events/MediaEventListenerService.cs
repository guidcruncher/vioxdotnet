using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;

namespace Viox.Core.Events;

/// <summary>
/// Listens to events from both Librespot and MPD event services, 
/// unifies them into a common MediaEvent format, and exposes a placeholder routing handler.
/// </summary>
public sealed class MediaEventListenerService : BackgroundService
{
    private readonly ILibrespotEventService _librespotEventService;
    private readonly IMpdEventService _mpdEventService;
    private readonly ILogger<MediaEventListenerService> _logger;
    private readonly IMediaEventService _mediaEventService;

    public MediaEventListenerService(
        ILibrespotEventService librespotEventService,
        IMpdEventService mpdEventService,
         IMediaEventService mediaEventService,
        ILogger<MediaEventListenerService> logger)
    {
        _mediaEventService = mediaEventService;
        _librespotEventService = librespotEventService;
        _mpdEventService = mpdEventService;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Unified Media Event Listener...");

        // Subscribe to Librespot Events
        _librespotEventService.OnNotPlayingReceived += HandleLibrespotNotPlayingReceived;
        _librespotEventService.OnActiveReceived += HandleLibrespotActive;
        _librespotEventService.OnInactiveReceived += HandleLibrespotInactive;
        _librespotEventService.OnMetadataReceived += HandleLibrespotMetadata;
        _librespotEventService.OnWillPlayReceived += HandleLibrespotWillPlay;
        _librespotEventService.OnPlayingReceived += HandleLibrespotPlaying;
        _librespotEventService.OnPausedReceived += HandleLibrespotPaused;
        _librespotEventService.OnStoppedReceived += HandleLibrespotStopped;
        _librespotEventService.OnPlaybackErrorReceived += HandleLibrespotError;
        _librespotEventService.OnSeekReceived += HandleLibrespotSeek;
        _librespotEventService.OnVolumeReceived += HandleLibrespotVolume;
        _librespotEventService.OnShuffleContextReceived += HandleLibrespotShuffle;
        _librespotEventService.OnRepeatContextReceived += HandleLibrespotRepeatContext;
        _librespotEventService.OnRepeatTrackReceived += HandleLibrespotRepeatTrack;

        // Subscribe to MPD Events
        _mpdEventService.OnTrackStartedReceived += HandleMpdTrackStarted;
        _mpdEventService.OnTrackFinishedReceived += HandleMpdTrackFinished;
        _mpdEventService.OnPlaybackStartedReceived += HandleMpdPlaybackStarted;
        _mpdEventService.OnPlaybackPausedReceived += HandleMpdPlaybackPaused;
        _mpdEventService.OnSeekedReceived += HandleMpdSeeked;
        _mpdEventService.OnVolumeReceived += HandleMpdVolume;
        _mpdEventService.OnShuffleChangedReceived += HandleMpdShuffle;
        _mpdEventService.OnRepeatChangedReceived += HandleMpdRepeat;

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Unified Media Event Listener...");

        // Unsubscribe Librespot
        _librespotEventService.OnNotPlayingReceived -= HandleLibrespotNotPlayingReceived;
        _librespotEventService.OnActiveReceived -= HandleLibrespotActive;
        _librespotEventService.OnInactiveReceived -= HandleLibrespotInactive;
        _librespotEventService.OnMetadataReceived -= HandleLibrespotMetadata;
        _librespotEventService.OnWillPlayReceived -= HandleLibrespotWillPlay;
        _librespotEventService.OnPlayingReceived -= HandleLibrespotPlaying;
        _librespotEventService.OnPausedReceived -= HandleLibrespotPaused;
        _librespotEventService.OnStoppedReceived -= HandleLibrespotStopped;
        _librespotEventService.OnPlaybackErrorReceived -= HandleLibrespotError;
        _librespotEventService.OnSeekReceived -= HandleLibrespotSeek;
        _librespotEventService.OnVolumeReceived -= HandleLibrespotVolume;
        _librespotEventService.OnShuffleContextReceived -= HandleLibrespotShuffle;
        _librespotEventService.OnRepeatContextReceived -= HandleLibrespotRepeatContext;
        _librespotEventService.OnRepeatTrackReceived -= HandleLibrespotRepeatTrack;

        // Unsubscribe MPD
        _mpdEventService.OnTrackStartedReceived -= HandleMpdTrackStarted;
        _mpdEventService.OnTrackFinishedReceived -= HandleMpdTrackFinished;
        _mpdEventService.OnPlaybackStartedReceived -= HandleMpdPlaybackStarted;
        _mpdEventService.OnPlaybackPausedReceived -= HandleMpdPlaybackPaused;
        _mpdEventService.OnSeekedReceived -= HandleMpdSeeked;
        _mpdEventService.OnVolumeReceived -= HandleMpdVolume;
        _mpdEventService.OnShuffleChangedReceived -= HandleMpdShuffle;
        _mpdEventService.OnRepeatChangedReceived -= HandleMpdRepeat;

        return base.StopAsync(cancellationToken);
    }

#-- Librespot Event Handlers --#

    private void HandleLibrespotActive(object? sender, ActiveEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.DeviceStateChanged, "librespot", null, new { Active = true }, DateTimeOffset.UtcNow));

    private void HandleLibrespotInactive(object? sender, InactiveEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.DeviceStateChanged, "librespot", null, new { Active = false }, DateTimeOffset.UtcNow));

    private void HandleLibrespotMetadata(object? sender, MetadataEvent e)
    {
        var track = new MediaMetaData
        {
            Title = e.Name,
            Artist = string.Join(", ", e.ArtistNames),
            Album = e.AlbumName,
            Uri = MediaUriParse.ParseMediaUriValue(e.Uri),
            ImageUrl = e.AlbumCoverUrl
        };
        PublishUnified(new MediaEvent(MediaEventType.TrackStarted, "librespot", track, e, DateTimeOffset.UtcNow));
    }

    private void HandleLibrespotNotPlayingReceived(object? sender, NotPlaying e) =>
        PublishUnified(new MediaEvent(MediaEventType.TrackFinished, "librespot", new MediaMetaData { Uri = MediaUriParse.ParseMediaUriValue(e.ContextUri) }, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotWillPlay(object? sender, WillPlayEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.TrackStarted, "librespot", new MediaMetaData { Uri = MediaUriParse.ParseMediaUriValue(e.Uri) }, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotPlaying(object? sender, PlayingEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.PlaybackStarted, "librespot", new MediaMetaData { Uri = MediaUriParse.ParseMediaUriValue(e.Uri) }, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotPaused(object? sender, PausedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.PlaybackPaused, "librespot", new MediaMetaData { Uri = MediaUriParse.ParseMediaUriValue(e.Uri) }, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotStopped(object? sender, StoppedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.PlaybackStopped, "librespot", null, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotError(object? sender, PlaybackErrorEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.PlaybackError, "librespot", new MediaMetaData { Uri = MediaUriParse.ParseMediaUriValue(e.Uri) }, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotSeek(object? sender, SeekEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.Seeked, "librespot", new MediaMetaData { Uri = MediaUriParse.ParseMediaUriValue(e.Uri) }, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotVolume(object? sender, VolumeEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.VolumeChanged, "librespot", null, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotShuffle(object? sender, ShuffleContextEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.ShuffleChanged, "librespot", null, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotRepeatContext(object? sender, RepeatContextEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.RepeatChanged, "librespot", null, e, DateTimeOffset.UtcNow));

    private void HandleLibrespotRepeatTrack(object? sender, RepeatTrackEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.RepeatChanged, "librespot", null, e, DateTimeOffset.UtcNow));

#-- MPD Event Handlers --#

    private void HandleMpdTrackStarted(object? sender, MpdTrackStartedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.TrackStarted, "mpd", e.Track, e, DateTimeOffset.UtcNow));

    private void HandleMpdTrackFinished(object? sender, MpdTrackFinishedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.TrackFinished, "mpd", e.Track, e, DateTimeOffset.UtcNow));

    private void HandleMpdPlaybackStarted(object? sender, MpdPlaybackStartedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.PlaybackStarted, "mpd", null, e, DateTimeOffset.UtcNow));

    private void HandleMpdPlaybackPaused(object? sender, MpdPlaybackPausedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.PlaybackPaused, "mpd", null, e, DateTimeOffset.UtcNow));

    private void HandleMpdSeeked(object? sender, MpdSeekedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.Seeked, "mpd", null, e, DateTimeOffset.UtcNow));

    private void HandleMpdVolume(object? sender, MpdVolumeEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.VolumeChanged, "mpd", null, e, DateTimeOffset.UtcNow));

    private void HandleMpdShuffle(object? sender, MpdShuffleChangedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.ShuffleChanged, "mpd", null, e, DateTimeOffset.UtcNow));

    private void HandleMpdRepeat(object? sender, MpdRepeatChangedEvent e) =>
        PublishUnified(new MediaEvent(MediaEventType.RepeatChanged, "mpd", null, e, DateTimeOffset.UtcNow));

    /// <summary>
    /// Placeholder routing function for processing or forwarding unified media events.
    /// </summary>
    private Task RouteUnifiedEventAsync(MediaEvent mediaEvent, CancellationToken ct = default)
    {
        await _mediaEventService.PublishMediaEventAsync(mediaEvent, ct);
        return Task.CompletedTask;
    }

    private void PublishUnified(MediaEvent mediaEvent)
    {
        _logger.LogInformation("Normalized Media Event [{Backend}] -> {Type}", mediaEvent.Backend, mediaEvent.Type);

        // Fire and forget or dispatch to placeholder router
        _ = RouteUnifiedEventAsync(mediaEvent);
    }
}
