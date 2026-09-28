using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Podverse.Configuration;
using Viox.Client.Podverse.Models;

namespace Viox.Client.Podverse.Services;

/// <summary>
/// Concrete implementation of <see cref="IPodverseClient"/> targeting .NET 10.
/// </summary>
public sealed class PodverseClient : IPodverseClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    private readonly HttpClient _httpClient;
    private readonly PodverseOptions _options;
    private readonly ILogger<PodverseClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodverseClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance.</param>
    /// <param name="options">The configured client options instance.</param>
    /// <param name="logger">The logger instance.</param>
    public PodverseClient(
        HttpClient httpClient,
        IOptions<PodverseOptions> options,
        ILogger<PodverseClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }

        if (!string.IsNullOrWhiteSpace(_options.AuthorizationToken))
        {
            _httpClient.DefaultRequestHeaders.Remove("authorization");
            _httpClient.DefaultRequestHeaders.Add("authorization", _options.AuthorizationToken);
        }
    }

    /// <inheritdoc />
    public async Task<Podcast?> GetPodcastByIdAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(podcastId);

        _logger.LogDebug("Executing GetPodcastById request for podcast ID: {PodcastId}", podcastId);

        try
        {
            return await _httpClient.GetFromJsonAsync<Podcast>($"podcast/{podcastId}", JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", podcastId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve podcast for ID: {PodcastId}", podcastId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Podcast>> GetPodcastsAsync(string? searchTitle = null, int? page = null, CancellationToken cancellationToken = default)
    {
        var pagedResult = await GetPodcastsPagedAsync(searchTitle, page, cancellationToken);
        return pagedResult?.Items ?? [];
    }

    /// <summary>
    /// Retrieves a paginated list of podcasts along with total count metadata.
    /// </summary>
    /// <param name="searchTitle">Optional search filter string.</param>
    /// <param name="page">Optional page index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PodversePagedResult{Podcast}"/> instance or null.</returns>
    public async Task<PodversePagedResult<Podcast>?> GetPodcastsPagedAsync(string? searchTitle = null, int? page = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(searchTitle))
        {
            queryParams.Add($"searchTitle={Uri.EscapeDataString(searchTitle)}");
        }
        if (page.HasValue)
        {
            queryParams.Add($"page={page.Value}");
        }

        var requestUri = "podcast";
        if (queryParams.Count > 0)
        {
            requestUri += "?" + string.Join("&", queryParams);
        }

        _logger.LogDebug("Executing GetPodcastsPaged request with URI: {RequestUri}", requestUri);

        return await _httpClient.GetFromJsonAsync<PodversePagedResult<Podcast>>(requestUri, JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Episode?> GetEpisodeByIdAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(episodeId);

        _logger.LogDebug("Executing GetEpisodeById request for episode ID: {EpisodeId}", episodeId);

        try
        {
            return await _httpClient.GetFromJsonAsync<Episode>($"episode/{episodeId}", JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Episode with ID {EpisodeId} was not found.", episodeId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve episode for ID: {EpisodeId}", episodeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetAuthenticatedUserInfoAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Executing GetAuthenticatedUserInfo request.");

        using var response = await _httpClient.PostAsync("auth/get-authenticated-user-info", null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized attempt accessing authenticated user details.");
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> TogglePodcastSubscriptionAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(podcastId);

        _logger.LogDebug("Executing TogglePodcastSubscription for podcast ID: {PodcastId}", podcastId);

        using var response = await _httpClient.GetAsync($"podcast/toggle-subscribe/{podcastId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
