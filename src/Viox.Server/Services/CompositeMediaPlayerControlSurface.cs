namespace Viox.Server.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Server.Abstraction;
using Viox.Server.Configuration;

/// <summary>
/// Control surface implementation managing routing based on playback URI or active player status.
/// </summary>
public sealed class CompositeMediaPlayerControlSurface : IMediaPlayerControlSurface
{
    private readonly IEnumerable<IMediaPlayerAdapter> _adapters;
    private readonly IOptions<MediaPlayerOptions> _options;
    private readonly ILogger<CompositeMediaPlayerControlSurface> _logger;
    private string? _lastActivePlayerName;
    private readonly ICurrentMediaService _currentMedia;
    private readonly MediaSourceResolverService _mediaResolver;
    private readonly AudioCacheManager _cacheManager;
    private readonly IMediaEventService _eventService;

    private string lastState = string.Empty;

    public CompositeMediaPlayerControlSurface(
        AudioCacheManager cacheManager,
        IEnumerable<IMediaPlayerAdapter> adapters,
        ICurrentMediaService currentMedia,
        MediaSourceResolverService mediaResolver,
        IOptions<MediaPlayerOptions> options,
        IMediaEventService eventService,
        ILogger<CompositeMediaPlayerControlSurface> logger)
    {
        _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _mediaResolver = mediaResolver ?? throw new ArgumentNullException(nameof(mediaResolver));
        _currentMedia = currentMedia ?? throw new ArgumentNullException(nameof(currentMedia));
        _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PlayAsync(string uri, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uri);

        MediaUri? mediaUri = uri.ParseMediaUri();
        if (mediaUri is null)
        {
            _logger.LogWarning("Unable to parse media URI string '{Uri}'. Playback aborted.", uri);
            return;
        }

        MediaMetaData? metaData = await _mediaResolver.ResolveMetaData(mediaUri, cancellationToken);
        if (metaData is null)
        {
            _logger.LogWarning("Unable to resolve media metadata for URI '{Uri}'. Playback aborted.", uri);
            return;
        }

        IMediaPlayerAdapter? targetAdapter = _adapters.FirstOrDefault(a => a.CanHandle(mediaUri.ToString()));
        if (targetAdapter is null)
        {
            throw new NotSupportedException($"No registered media engine adapter can handle the URI: '{uri}'.");
        }

        IMediaPlayerAdapter? currentActive = await GetActivePlayerAsync(cancellationToken);
        if (currentActive is not null && !string.Equals(currentActive.Name, targetAdapter.Name, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Stopping current active engine '{ActiveEngine}' before switching to '{NewEngine}'.", currentActive.Name, targetAdapter.Name);
            await currentActive.StopAsync(cancellationToken);
        }

        _currentMedia.SetCurrentMedia(metaData);
        _logger.LogInformation("Dispatching play command for URI '{Uri}', URL '{Url}' to engine '{Engine}'.", uri, metaData.Url, targetAdapter.Name);

        if (targetAdapter.UseProxy && mediaUri.Source == "podverse")
        {
            string? localFilePath = await _cacheManager.GetOrDownloadAudioAsync(metaData.Url, metaData.ReleaseDate, cancellationToken);
            if (string.IsNullOrEmpty(localFilePath) || !File.Exists(localFilePath))
            {
                return;
            }

            _logger.LogInformation("Playing local file '{File}'", localFilePath);
            await targetAdapter.PlayAsync(localFilePath, cancellationToken);
        }
        else
        {
            if (targetAdapter.Name == "Librespot")
            {
                await targetAdapter.PlayAsync(metaData.RawUri, cancellationToken);
            }
            else
            {
                await targetAdapter.PlayAsync(metaData.Url, cancellationToken);
            }
        }

        await _eventService.PublishMediaActionAsync("play", metaData, cancellationToken);
        _lastActivePlayerName = targetAdapter.Name;
    }

    public MediaMetaData? GetCurrentTrack() => _currentMedia.CurrentMedia;

    public async Task<PlaybackState> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        PlaybackState state = new();
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        MediaMetaData? metaData = GetCurrentTrack();
        IMediaPlayerAdapter? activePlayer = await GetActivePlayerAsync(cancellationToken);

        if (activePlayer is not null)
        {
            state.ActiveBackend = activePlayer.Name;
            state.Playing = true;
            if (metaData is not null)
            {
                state.Track = metaData;
                double? position = await activePlayer.GetPlaybackPositionAsync(cancellationToken);
                state.Position = position ?? 0;
                state.IsLive = state.Track.Duration is not null;
            }

            string currentState = JsonSerializer.Serialize(state, options);
            if (currentState != lastState)
            {
                await _eventService.PublishPlaybackStatusAsync(state, cancellationToken);
                lastState = currentState;
            }
        }
        else
        {
            state.Playing = false;
            if (!string.IsNullOrEmpty(lastState))
            {
                await _eventService.PublishPlaybackStatusAsync(state, cancellationToken);
                lastState = string.Empty;
            }
        }

        return state;
    }

    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        await adapter.PauseAsync(cancellationToken);
        await _eventService.PublishMediaActionAsync("pause", GetCurrentTrack(), cancellationToken);
    }

    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        await adapter.ResumeAsync(cancellationToken);
        await _eventService.PublishMediaActionAsync("resume", GetCurrentTrack(), cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        await adapter.StopAsync(cancellationToken);
        await _eventService.PublishMediaActionAsync("stop", GetCurrentTrack(), cancellationToken);
    }

    public async Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        await adapter.SeekAsync(position, cancellationToken);
        await _eventService.PublishMediaActionAsync("seek", GetCurrentTrack(), cancellationToken);
    }

    public async Task NextAsync(CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        await adapter.NextAsync(cancellationToken);
        await _eventService.PublishMediaActionAsync("next", GetCurrentTrack(), cancellationToken);
    }

    public async Task PreviousAsync(CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        await adapter.PreviousAsync(cancellationToken);
        await _eventService.PublishMediaActionAsync("previous", GetCurrentTrack(), cancellationToken);
    }

    public async Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default)
    {
        foreach (IMediaPlayerAdapter adapter in _adapters)
        {
            await adapter.SetVolumeAsync(volumePercent, cancellationToken);
        }
    }

    public async Task<int> GetVolumeAsync(CancellationToken cancellationToken = default)
    {
        IMediaPlayerAdapter adapter = await ResolveTargetAdapterAsync(cancellationToken);
        return await adapter.GetVolumeAsync(cancellationToken);
    }

    public async Task<IMediaPlayerAdapter?> GetActivePlayerAsync(CancellationToken cancellationToken = default)
    {
        foreach (IMediaPlayerAdapter adapter in _adapters)
        {
            if (await adapter.IsActiveAsync(cancellationToken))
            {
                _lastActivePlayerName = adapter.Name;
                return adapter;
            }
        }
        return null;
    }

    private async Task<IMediaPlayerAdapter> ResolveTargetAdapterAsync(CancellationToken cancellationToken)
    {
        IMediaPlayerAdapter? active = await GetActivePlayerAsync(cancellationToken);
        if (active is not null)
        {
            return active;
        }

        if (!string.IsNullOrEmpty(_lastActivePlayerName))
        {
            IMediaPlayerAdapter? last = _adapters.FirstOrDefault(a => string.Equals(a.Name, _lastActivePlayerName, StringComparison.OrdinalIgnoreCase));
            if (last is not null)
            {
                return last;
            }
        }

        string defaultName = _options.Value.DefaultPlayerName;
        IMediaPlayerAdapter? fallback = _adapters.FirstOrDefault(a => string.Equals(a.Name, defaultName, StringComparison.OrdinalIgnoreCase))
                                       ?? _adapters.FirstOrDefault();

        return fallback ?? throw new InvalidOperationException("No media player adapters are registered in the control surface.");
    }
}
