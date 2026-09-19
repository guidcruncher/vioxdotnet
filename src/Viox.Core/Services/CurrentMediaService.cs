namespace Viox.Core.Services;

using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;

/// <summary>
/// Singleton service managing global active media playback state.
/// </summary>
public sealed class CurrentMediaService : ICurrentMediaService
{
    private readonly ILogger<CurrentMediaService> _logger;
    private readonly CurrentMediaServiceOptions _options;
    private readonly Lock _syncLock = new();
    private MediaMetaData? _currentMedia;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentMediaService"/> class.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger implementation.</param>
    public CurrentMediaService(
        IOptions<CurrentMediaServiceOptions> options,
        ILogger<CurrentMediaService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value;
    }

    /// <inheritdoc />
    public event EventHandler<MediaChangedEventArgs>? CurrentMediaChanged;

    /// <inheritdoc />
    public MediaMetaData? CurrentMedia
    {
        get
        {
            lock (_syncLock)
            {
                return _currentMedia;
            }
        }
    }

    /// <inheritdoc />
    public void SetCurrentMedia(MediaMetaData? mediaMetaData)
    {
        MediaMetaData? previousState;

        lock (_syncLock)
        {
            previousState = _currentMedia;
            _currentMedia = mediaMetaData;
        }

        if (_options.EnableStateChangeLogging)
        {
            if (mediaMetaData is not null)
            {
                _logger.LogInformation(
                    "Global current media updated: Title='{Title}', Artist='{Artist}', Uri='{Uri}'",
                    mediaMetaData.Title,
                    mediaMetaData.Artist,
                    mediaMetaData.Uri?.ToString() ?? "N/A");
            }
            else if (previousState is not null)
            {
                _logger.LogInformation("Global current media cleared.");
            }
        }

        CurrentMediaChanged?.Invoke(this, new MediaChangedEventArgs(mediaMetaData));
    }

    /// <inheritdoc />
    public void ClearCurrentMedia()
    {
        SetCurrentMedia(null);
    }
}
