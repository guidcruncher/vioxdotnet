using System.Net.Http.Json;
using System.Text.Encodings.Web;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.TuneIn.Configuration;
using Viox.Client.TuneIn.Models;

namespace Viox.Client.TuneIn.Services;

/// <summary>
/// Default implementation of <see cref="ITuneInClient"/> for communication with TuneIn API endpoints.
/// </summary>
public sealed class TuneInClient : ITuneInClient
{
    private readonly HttpClient _httpClient;
    private readonly TuneInOptions _options;
    private readonly ILogger<TuneInClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TuneInClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance configured for TuneIn.</param>
    /// <param name="options">Options instance containing client settings.</param>
    /// <param name="logger">Logger instance for standard Microsoft logging.</param>
    public TuneInClient(
        HttpClient httpClient,
        IOptions<TuneInOptions> options,
        ILogger<TuneInClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<TuneInResponse<TuneInOutline>?> BrowseAsync(
        string? category = null,
        string? id = null,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(category))
        {
            parameters["c"] = category;
        }

        if (!string.IsNullOrWhiteSpace(id))
        {
            parameters["id"] = id;
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            parameters["filter"] = filter;
        }

        return ExecuteApiCallAsync<TuneInOutline>("Browse.ashx", parameters, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TuneInResponse<StationElement>?> DescribeAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var parameters = new Dictionary<string, string?>
        {
            ["id"] = id
        };

        return ExecuteApiCallAsync<StationElement>("Describe.ashx", parameters, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TuneInResponse<TuneInOutline>?> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var parameters = new Dictionary<string, string?>
        {
            ["query"] = query
        };

        return ExecuteApiCallAsync<TuneInOutline>("Search.ashx", parameters, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TuneInResponse<AudioElement>?> TuneAsync(
        string id,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var parameters = new Dictionary<string, string?>
        {
            ["id"] = id
        };

        if (!string.IsNullOrWhiteSpace(filter))
        {
            parameters["filter"] = filter;
        }

        return ExecuteApiCallAsync<AudioElement>("Tune.ashx", parameters, cancellationToken);
    }

    private async Task<TuneInResponse<T>?> ExecuteApiCallAsync<T>(
        string endpointPath,
        Dictionary<string, string?> queryParameters,
        CancellationToken cancellationToken)
    {
        queryParameters["render"] = "json";

        if (!string.IsNullOrWhiteSpace(_options.PartnerId))
        {
            queryParameters["partnerId"] = _options.PartnerId;
        }

        var queryString = string.Join("&", queryParameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{UrlEncoder.Default.Encode(p.Key)}={UrlEncoder.Default.Encode(p.Value!)}"));

        var requestUri = $"{endpointPath}?{queryString}";

        _logger.LogInformation("Executing TuneIn API request: {EndpointPath}", endpointPath);

        try
        {
            var response = await _httpClient.GetFromJsonAsync<TuneInResponse<T>>(requestUri, cancellationToken);
            _logger.LogDebug("Successfully retrieved response from {EndpointPath}", endpointPath);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Request error while executing endpoint {EndpointPath}", endpointPath);
            throw;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout exceeded while calling endpoint {EndpointPath}", endpointPath);
            throw;
        }
    }
}
