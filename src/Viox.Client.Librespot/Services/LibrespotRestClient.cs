using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Librespot.Configuration;
using Viox.Client.Librespot.Models;

namespace Viox.Client.Librespot.Services;

/// <summary>
/// Fully compliant HTTP client for go-librespot daemon REST API.
/// </summary>
public sealed class LibrespotRestClient : ILibrespotRestClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LibrespotRestClient> _logger;

    public LibrespotRestClient(HttpClient httpClient, IOptions<LibrespotOptions> options, ILogger<LibrespotRestClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var opts = options.Value;
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(opts.BaseUrl);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
    }

    public async Task<ApiRoot?> GetRootAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonOrNullAsync<ApiRoot>("/", cancellationToken);
    }

    public async Task<ApiStatus?> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonOrNullAsync<ApiStatus>("/status", cancellationToken);
    }

    public async Task<ApiToken?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        return await PostJsonOrNullAsync<ApiToken>("/token", cancellationToken);
    }

    public async Task<ApiDeviceAuth?> GetAuthCodeAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonOrNullAsync<ApiDeviceAuth>("/auth/code", cancellationToken);
    }

    public async Task SetDeviceNameAsync(ApiSetDeviceNameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/set_device_name", request, cancellationToken);
    }

    public async Task PlayAsync(ApiPlayRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/play", request, cancellationToken);
    }

    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        await PostAsync("/player/resume", cancellationToken);
    }

    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        await PostAsync("/player/pause", cancellationToken);
    }

    public async Task PlayPauseAsync(CancellationToken cancellationToken = default)
    {
        await PostAsync("/player/playpause", cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await PostAsync("/player/stop", cancellationToken);
    }

    public async Task SetOutputAsync(ApiOutputRequest? request = null, CancellationToken cancellationToken = default)
    {
        await PostJsonAsync("/player/output", request ?? new ApiOutputRequest(), cancellationToken);
    }

    public async Task NextAsync(ApiNextRequest? request = null, CancellationToken cancellationToken = default)
    {
        await PostJsonAsync("/player/next", request ?? new ApiNextRequest(), cancellationToken);
    }

    public async Task PreviousAsync(CancellationToken cancellationToken = default)
    {
        await PostAsync("/player/prev", cancellationToken);
    }

    public async Task SeekAsync(ApiSeekRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/seek", request, cancellationToken);
    }

    public async Task<ApiVolume?> GetVolumeAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonOrNullAsync<ApiVolume>("/player/volume", cancellationToken);
    }

    public async Task SetVolumeAsync(ApiSetVolumeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/volume", request, cancellationToken);
    }

    public async Task SetRepeatContextAsync(ApiRepeatContextRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/repeat_context", request, cancellationToken);
    }

    public async Task SetRepeatTrackAsync(ApiRepeatTrackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/repeat_track", request, cancellationToken);
    }

    public async Task SetShuffleContextAsync(ApiShuffleContextRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/shuffle_context", request, cancellationToken);
    }

    public async Task AddToQueueAsync(ApiAddToQueueRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostJsonAsync("/player/add_to_queue", request, cancellationToken);
    }

    private async Task<T?> GetJsonOrNullAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP GET to '{RequestUri}' failed.", requestUri);
            throw;
        }
    }

    private async Task PostAsync(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsync(requestUri, null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP POST to '{RequestUri}' failed.", requestUri);
            throw;
        }
    }

    private async Task PostJsonAsync<TRequest>(string requestUri, TRequest payload, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP POST JSON to '{RequestUri}' failed.", requestUri);
            throw;
        }
    }

    private async Task<T?> PostJsonOrNullAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsync(requestUri, null, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP POST to '{RequestUri}' failed.", requestUri);
            throw;
        }
    }
}
