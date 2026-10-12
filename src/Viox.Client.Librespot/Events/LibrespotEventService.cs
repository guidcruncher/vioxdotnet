using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace Viox.Client.Librespot.Events;

public class LibrespotEventService : ILibrespotEventService
{
    private readonly ILogger<LibrespotEventService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public event EventHandler<LibrespotEvent>? OnEventReceived;
    public event EventHandler<ActiveEvent>? OnActiveReceived;
    public event EventHandler<InactiveEvent>? OnInactiveReceived;
    public event EventHandler<MetadataEvent>? OnMetadataReceived;
    public event EventHandler<WillPlayEvent>? OnWillPlayReceived;
    public event EventHandler<PlayingEvent>? OnPlayingReceived;
    public event EventHandler<NotPlayingEvent>? OnNotPlayingReceived;
    public event EventHandler<PausedEvent>? OnPausedReceived;
    public event EventHandler<StoppedEvent>? OnStoppedReceived;
    public event EventHandler<PlaybackErrorEvent>? OnPlaybackErrorReceived;
    public event EventHandler<SeekEvent>? OnSeekReceived;
    public event EventHandler<VolumeEvent>? OnVolumeReceived;
    public event EventHandler<ShuffleContextEvent>? OnShuffleContextReceived;
    public event EventHandler<RepeatContextEvent>? OnRepeatContextReceived;
    public event EventHandler<RepeatTrackEvent>? OnRepeatTrackReceived;

    public LibrespotEventService(ILogger<LibrespotEventService> logger)
    {
        _logger = logger;
    }

    public void DispatchEvent(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (!doc.RootElement.TryGetProperty("type", out var typeProp))
            {
                _logger.LogWarning("Received websocket message without type property: {Json}", rawJson);
                return;
            }

            var eventType = typeProp.GetString();
            var baseEvent = new LibrespotEvent(eventType ?? "unknown");
            OnEventReceived?.Invoke(this, baseEvent);

            switch (eventType)
            {
                case "active":
                    OnActiveReceived?.Invoke(this, new ActiveEvent());
                    break;
                case "inactive":
                    OnInactiveReceived?.Invoke(this, new InactiveEvent());
                    break;
                case "metadata":
                    var meta = JsonSerializer.Deserialize<MetadataEvent>(rawJson, JsonOptions);
                    if (meta != null) OnMetadataReceived?.Invoke(this, meta);
                    break;
                case "will_play":
                    var willPlay = JsonSerializer.Deserialize<WillPlayEvent>(rawJson, JsonOptions);
                    if (willPlay != null) OnWillPlayReceived?.Invoke(this, willPlay);
                    break;
                case "playing":
                    var playing = JsonSerializer.Deserialize<PlayingEvent>(rawJson, JsonOptions);
                    if (playing != null) OnPlayingReceived?.Invoke(this, playing);
                    break;
                case "not_playing":
                    var notPlaying = JsonSerializer.Deserialize<NotPlayingEvent>(rawJson, JsonOptions);
                    if (notPlaying != null) OnNotPlayingReceived?.Invoke(this, notPlaying);
                    break;
                case "paused":
                    var paused = JsonSerializer.Deserialize<PausedEvent>(rawJson, JsonOptions);
                    if (paused != null) OnPausedReceived?.Invoke(this, paused);
                    break;
                case "stopped":
                    var stopped = JsonSerializer.Deserialize<StoppedEvent>(rawJson, JsonOptions);
                    if (stopped != null) OnStoppedReceived?.Invoke(this, stopped);
                    break;
                case "playback_error":
                    var err = JsonSerializer.Deserialize<PlaybackErrorEvent>(rawJson, JsonOptions);
                    if (err != null) OnPlaybackErrorReceived?.Invoke(this, err);
                    break;
                case "seek":
                    var seek = JsonSerializer.Deserialize<SeekEvent>(rawJson, JsonOptions);
                    if (seek != null) OnSeekReceived?.Invoke(this, seek);
                    break;
                case "volume":
                    var vol = JsonSerializer.Deserialize<VolumeEvent>(rawJson, JsonOptions);
                    if (vol != null) OnVolumeReceived?.Invoke(this, vol);
                    break;
                case "shuffle_context":
                    var shuffle = JsonSerializer.Deserialize<ShuffleContextEvent>(rawJson, JsonOptions);
                    if (shuffle != null) OnShuffleContextReceived?.Invoke(this, shuffle);
                    break;
                case "repeat_context":
                    var repCtx = JsonSerializer.Deserialize<RepeatContextEvent>(rawJson, JsonOptions);
                    if (repCtx != null) OnRepeatContextReceived?.Invoke(this, repCtx);
                    break;
                case "repeat_track":
                    var repTrk = JsonSerializer.Deserialize<RepeatTrackEvent>(rawJson, JsonOptions);
                    if (repTrk != null) OnRepeatTrackReceived?.Invoke(this, repTrk);
                    break;
                default:
                    _logger.LogDebug("Unhandled go-librespot event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse incoming librespot websocket payload: {Json}", rawJson);
        }
    }
}
