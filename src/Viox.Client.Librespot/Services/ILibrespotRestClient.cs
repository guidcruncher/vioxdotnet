using Viox.Client.Librespot.Models;

namespace Viox.Client.Librespot.Services;

/// <summary>
/// Full contract for interacting with all endpoints in go-librespot OpenAPI specification.
/// </summary>
public interface ILibrespotRestClient
{
    Task<ApiRoot?> GetRootAsync(CancellationToken cancellationToken = default);
    Task<ApiStatus?> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<ApiToken?> GetTokenAsync(CancellationToken cancellationToken = default);
    Task<ApiDeviceAuth?> GetAuthCodeAsync(CancellationToken cancellationToken = default);
    Task SetDeviceNameAsync(ApiSetDeviceNameRequest request, CancellationToken cancellationToken = default);
    Task PlayAsync(ApiPlayRequest request, CancellationToken cancellationToken = default);
    Task ResumeAsync(CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
    Task PlayPauseAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task SetOutputAsync(ApiOutputRequest? request = null, CancellationToken cancellationToken = default);
    Task NextAsync(ApiNextRequest? request = null, CancellationToken cancellationToken = default);
    Task PreviousAsync(CancellationToken cancellationToken = default);
    Task SeekAsync(ApiSeekRequest request, CancellationToken cancellationToken = default);
    Task<ApiVolume?> GetVolumeAsync(CancellationToken cancellationToken = default);
    Task SetVolumeAsync(ApiSetVolumeRequest request, CancellationToken cancellationToken = default);
    Task SetRepeatContextAsync(ApiRepeatContextRequest request, CancellationToken cancellationToken = default);
    Task SetRepeatTrackAsync(ApiRepeatTrackRequest request, CancellationToken cancellationToken = default);
    Task SetShuffleContextAsync(ApiShuffleContextRequest request, CancellationToken cancellationToken = default);
    Task AddToQueueAsync(ApiAddToQueueRequest request, CancellationToken cancellationToken = default);
}

