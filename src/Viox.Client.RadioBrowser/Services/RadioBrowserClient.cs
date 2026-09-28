using System.Net.Http.Headers;
using System.Text.Json;

using Viox.Client.RadioBrowser.Configuration;
using Viox.Client.RadioBrowser.Models;

namespace Viox.Client.RadioBrowser.Services;

/// <summary>
/// HttpClient-based implementation of <see cref="IRadioBrowserClient"/>.
/// </summary>
/// <remarks>
/// Radio Browser is a public community directory. Send a descriptive <c>User-Agent</c>, prefer HTTPS,
/// and call <see cref="ClickStationAsync"/> when playback starts so popularity metrics stay accurate.
/// </remarks>
public sealed class RadioBrowserClient : IRadioBrowserClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a client that owns its <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="options">Optional configuration. A default user agent and German mirror are used when omitted.</param>
    public RadioBrowserClient(RadioBrowserOptions? options = null)
        : this(CreateHttpClient(options ?? new RadioBrowserOptions()), options ?? new RadioBrowserOptions(), ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Creates a client that uses an existing <see cref="HttpClient"/>, for example one from
    /// <c>IHttpClientFactory</c>. The handler pipeline and timeout stay under the caller's control.
    /// </summary>
    /// <param name="httpClient">HTTP client whose <see cref="HttpClient.BaseAddress"/> should point at a Radio Browser mirror.</param>
    /// <param name="options">Optional configuration used for the user agent and JSON settings.</param>
    public RadioBrowserClient(HttpClient httpClient, RadioBrowserOptions? options = null)
        : this(httpClient, options ?? new RadioBrowserOptions(), ownsHttpClient: false)
    {
    }

    private RadioBrowserClient(HttpClient httpClient, RadioBrowserOptions options, bool ownsHttpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;
        _jsonOptions = CreateJsonOptions(options);

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = options.BaseAddress ?? options.FallbackBaseAddress;
        }

        ApplyUserAgent(_httpClient, options.UserAgent);
    }

    /// <inheritdoc />
    public Uri BaseAddress => _httpClient.BaseAddress
        ?? throw new InvalidOperationException("The HTTP client has no BaseAddress.");

    /// <inheritdoc />
    public Task<ServerStats> GetStatsAsync(CancellationToken cancellationToken = default)
        => GetRequiredAsync<ServerStats>("json/stats", cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<ApiServer>> GetServersAsync(CancellationToken cancellationToken = default)
        => GetListAsync<ApiServer>("json/servers", cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> SearchStationsAsync(
        StationSearchOptions? options = null,
        CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations/search", QueryString.FromSearchOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsAsync(
        ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations", QueryString.FromListOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByUuidAsync(
        IEnumerable<string> stationUuids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stationUuids);
        var joined = string.Join(',', stationUuids.Where(static id => !string.IsNullOrWhiteSpace(id)).Select(static id => id.Trim()));
        if (string.IsNullOrWhiteSpace(joined))
        {
            throw new ArgumentException("At least one station UUID is required.", nameof(stationUuids));
        }

        var path = joined.Contains(',', StringComparison.Ordinal)
            ? QueryString.Combine("json/stations/byuuid", QueryString.Build(("uuids", joined)))
            : $"json/stations/byuuid/{QueryString.EscapePath(joined)}";

        return GetListAsync<Station>(path, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Station?> GetStationByUuidAsync(string stationUuid, CancellationToken cancellationToken = default)
    {
        var stations = await GetStationsByUuidAsync([stationUuid], cancellationToken).ConfigureAwait(false);
        return stations.Count == 0 ? null : stations[0];
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByNameAsync(string name, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("byname", name, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByNameExactAsync(string name, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bynameexact", name, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByTagAsync(string tag, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bytag", tag, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByTagExactAsync(string tag, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bytagexact", tag, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByCountryAsync(string country, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bycountry", country, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByCountryExactAsync(string country, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bycountryexact", country, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByCountryCodeAsync(string countryCode, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bycountrycodeexact", countryCode, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByStateAsync(string state, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bystate", state, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByStateExactAsync(string state, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bystateexact", state, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByLanguageAsync(string language, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bylanguage", language, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByLanguageExactAsync(string language, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bylanguageexact", language, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByCodecAsync(string codec, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bycodec", codec, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByCodecExactAsync(string codec, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetStationsByAttributeAsync("bycodecexact", codec, options, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetStationsByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return GetListAsync<Station>(
            QueryString.Combine("json/stations/byurl", QueryString.Build(("url", url))),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetTopClickedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations/topclick", QueryString.FromListOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetTopVotedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations/topvote", QueryString.FromListOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetRecentlyClickedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations/lastclick", QueryString.FromListOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetRecentlyChangedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations/lastchange", QueryString.FromListOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Station>> GetBrokenStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<Station>(QueryString.Combine("json/stations/broken", QueryString.FromListOptions(options)), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<CountryInfo>> GetCountriesAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<CountryInfo>(BuildFilteredPath("json/countries", filter, options), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<CountryCodeInfo>> GetCountryCodesAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<CountryCodeInfo>(BuildFilteredPath("json/countrycodes", filter, options), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<StateInfo>> GetStatesAsync(string? country = null, string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        string path;
        if (!string.IsNullOrWhiteSpace(country) && !string.IsNullOrWhiteSpace(filter))
        {
            path = $"json/states/{QueryString.EscapePath(country)}/{QueryString.EscapePath(filter)}";
        }
        else if (!string.IsNullOrWhiteSpace(filter))
        {
            path = $"json/states/{QueryString.EscapePath(filter)}";
        }
        else
        {
            path = "json/states";
        }

        var paging = QueryString.FromListOptions(options);
        var countryQuery = string.IsNullOrWhiteSpace(filter) && !string.IsNullOrWhiteSpace(country)
            ? QueryString.Build(("country", country))
            : string.Empty;

        var query = MergeQueries(paging, countryQuery);
        return GetListAsync<StateInfo>(QueryString.Combine(path, query), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LanguageInfo>> GetLanguagesAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<LanguageInfo>(BuildFilteredPath("json/languages", filter, options), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TagInfo>> GetTagsAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<TagInfo>(BuildFilteredPath("json/tags", filter, options), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<CodecInfo>> GetCodecsAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default)
        => GetListAsync<CodecInfo>(BuildFilteredPath("json/codecs", filter, options), cancellationToken);

    /// <inheritdoc />
    public Task<ClickResult> ClickStationAsync(string stationUuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stationUuid);
        return GetRequiredAsync<ClickResult>($"json/url/{QueryString.EscapePath(stationUuid)}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<VoteResult> VoteStationAsync(string stationUuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stationUuid);
        return GetRequiredAsync<VoteResult>($"json/vote/{QueryString.EscapePath(stationUuid)}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AddStationResult> AddStationAsync(AddStationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Url);

        using var content = QueryString.ToForm(
        [
            ("name", request.Name),
            ("url", request.Url),
            ("homepage", request.Homepage),
            ("favicon", request.Favicon),
            ("countrycode", request.CountryCode),
            ("state", request.State),
            ("language", request.Language),
            ("tags", request.Tags),
            ("geo_lat", QueryString.ToApiDouble(request.GeoLatitude)),
            ("geo_long", QueryString.ToApiDouble(request.GeoLongitude))
        ]);

        return await SendRequiredAsync<AddStationResult>(HttpMethod.Post, "json/add", content, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StationClick>> GetClicksAsync(string? stationUuid = null, int? seconds = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(stationUuid)
            ? "json/clicks"
            : $"json/clicks/{QueryString.EscapePath(stationUuid)}";
        var query = QueryString.Build(("seconds", QueryString.ToApiInt(seconds)));
        return GetListAsync<StationClick>(QueryString.Combine(path, query), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StationCheck>> GetChecksAsync(string? stationUuid = null, int? lastCheckTime = null, int? seconds = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(stationUuid)
            ? "json/checks"
            : $"json/checks/{QueryString.EscapePath(stationUuid)}";
        var query = QueryString.Build(
            ("lastchecktime", QueryString.ToApiInt(lastCheckTime)),
            ("seconds", QueryString.ToApiInt(seconds)));
        return GetListAsync<StationCheck>(QueryString.Combine(path, query), cancellationToken);
    }

    /// <inheritdoc />
    public Uri GetPlaylistUri(string format, string relativePathAndQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePathAndQuery);
        var trimmedFormat = format.Trim().Trim('/');
        var relative = relativePathAndQuery.TrimStart('/');
        return new Uri(BaseAddress, $"{trimmedFormat}/{relative}");
    }

    /// <summary>
    /// Releases the owned <see cref="HttpClient"/> when this instance created it.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private Task<IReadOnlyList<Station>> GetStationsByAttributeAsync(
        string attribute,
        string value,
        ListQueryOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var path = $"json/stations/{attribute}/{QueryString.EscapePath(value)}";
        return GetListAsync<Station>(QueryString.Combine(path, QueryString.FromListOptions(options)), cancellationToken);
    }

    private static string MergeQueries(params string[] parts)
    {
        var values = parts
            .Where(static part => !string.IsNullOrWhiteSpace(part))
            .Select(static part => part.Trim().TrimStart('?').TrimStart('&'))
            .Where(static part => part.Length > 0)
            .ToArray();

        return values.Length == 0 ? string.Empty : "?" + string.Join('&', values);
    }

    private static string BuildFilteredPath(string root, string? filter, ListQueryOptions? options)
    {
        var path = string.IsNullOrWhiteSpace(filter) ? root : $"{root}/{QueryString.EscapePath(filter)}";
        return QueryString.Combine(path, QueryString.FromListOptions(options));
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        var items = await GetAsync<List<T>>(relativeUri, cancellationToken).ConfigureAwait(false);
        return items ?? [];
    }

    private async Task<T> GetRequiredAsync<T>(string relativeUri, CancellationToken cancellationToken)
        where T : class
    {
        var value = await GetAsync<T>(relativeUri, cancellationToken).ConfigureAwait(false);
        return value ?? throw new RadioBrowserException(
            $"The Radio Browser API returned an empty {typeof(T).Name} payload.",
            requestUri: new Uri(BaseAddress, relativeUri));
    }

    private Task<T?> GetAsync<T>(string relativeUri, CancellationToken cancellationToken)
        => SendAsync<T>(HttpMethod.Get, relativeUri, content: null, cancellationToken);

    private async Task<T> SendRequiredAsync<T>(HttpMethod method, string relativeUri, HttpContent? content, CancellationToken cancellationToken)
        where T : class
    {
        var value = await SendAsync<T>(method, relativeUri, content, cancellationToken).ConfigureAwait(false);
        return value ?? throw new RadioBrowserException(
            $"The Radio Browser API returned an empty {typeof(T).Name} payload.",
            requestUri: new Uri(BaseAddress, relativeUri));
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string relativeUri, HttpContent? content, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, relativeUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (content is not null)
        {
            request.Content = content;
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RadioBrowserException(
                $"Failed to call the Radio Browser API at '{relativeUri}'.",
                requestUri: SafeCombine(relativeUri),
                innerException: ex);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new RadioBrowserException(
                    $"Radio Browser API request failed with {(int)response.StatusCode} ({response.StatusCode}).",
                    response.StatusCode,
                    response.RequestMessage?.RequestUri ?? SafeCombine(relativeUri),
                    body);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(body, _jsonOptions);
            }
            catch (JsonException ex)
            {
                throw new RadioBrowserException(
                    $"Failed to deserialize Radio Browser response as {typeof(T).Name}.",
                    response.StatusCode,
                    response.RequestMessage?.RequestUri ?? SafeCombine(relativeUri),
                    body,
                    ex);
            }
        }
    }

    private Uri? SafeCombine(string relativeUri)
    {
        try
        {
            return new Uri(BaseAddress, relativeUri);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static HttpClient CreateHttpClient(RadioBrowserOptions options)
    {
        return new HttpClient
        {
            BaseAddress = options.BaseAddress ?? options.FallbackBaseAddress,
            Timeout = options.Timeout
        };
    }

    private static void ApplyUserAgent(HttpClient httpClient, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = RadioBrowserOptions.DefaultUserAgent;
        }

        httpClient.DefaultRequestHeaders.UserAgent.Clear();
        if (!httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent))
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
        }
    }

    internal static JsonSerializerOptions CreateJsonOptions(RadioBrowserOptions options)
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }
}
