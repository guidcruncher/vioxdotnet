using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Youtube.Configuration;

namespace Viox.Client.Youtube.Internal;

internal sealed class InnerTubeClient
{
    internal const string DefaultApiKey = "AIzaSyC9XL3ZjWddXya6X74dJoCTL-WEYFDNX30";
    internal const string FallbackClientVersion = "1.20260927.17.00";

    private static readonly Uri HomeUri = new("https://music.youtube.com/");
    private static readonly Uri SearchUri = new("https://music.youtube.com/youtubei/v1/search?prettyPrint=false");
    private static readonly Uri NextUri = new("https://music.youtube.com/youtubei/v1/next?prettyPrint=false");
    private static readonly Uri PlayerUri = new("https://music.youtube.com/youtubei/v1/player?prettyPrint=false");
    private static readonly Uri BrowseUri = new("https://music.youtube.com/youtubei/v1/browse?prettyPrint=false");

    private readonly HttpClient _http;
    private readonly ILogger<InnerTubeClient> _logger;
    private readonly YoutubeMusicOptions _options;
    private readonly SemaphoreSlim _bootstrapLock = new(1, 1);

    private string? _clientVersion;
    private string? _apiKey;
    private string? _visitorData;

    public InnerTubeClient(
        HttpClient http,
        IOptions<YoutubeMusicOptions> options,
        ILogger<InnerTubeClient> logger)
    {
        _http = http;
        _logger = logger;
        _options = options.Value;

        _http.Timeout = _options.RequestTimeout;
        _http.DefaultRequestHeaders.UserAgent.Clear();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://music.youtube.com");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://music.youtube.com/");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
    }

    public async Task<JsonDocument> SearchAsync(string query, string? filterParams, CancellationToken cancellationToken)
    {
        var context = await CreateContextAsync(cancellationToken).ConfigureAwait(false);
        using var payload = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("context");
            context.WriteTo(writer);
            writer.WriteString("query", query);
            if (!string.IsNullOrWhiteSpace(filterParams))
            {
                writer.WriteString("params", filterParams);
            }

            writer.WriteEndObject();
        }

        return await PostAsync(SearchUri, payload.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonDocument> NextAsync(string videoId, CancellationToken cancellationToken)
    {
        var context = await CreateContextAsync(cancellationToken).ConfigureAwait(false);
        using var payload = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("context");
            context.WriteTo(writer);
            writer.WriteString("videoId", videoId);
            writer.WriteBoolean("isAudioOnly", true);
            writer.WriteEndObject();
        }

        return await PostAsync(NextUri, payload.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonDocument> PlayerAsync(string videoId, CancellationToken cancellationToken)
    {
        var context = await CreateContextAsync(cancellationToken).ConfigureAwait(false);
        using var payload = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("context");
            context.WriteTo(writer);
            writer.WriteString("videoId", videoId);
            writer.WriteBoolean("contentCheckOk", true);
            writer.WriteBoolean("racyCheckOk", true);
            writer.WriteEndObject();
        }

        return await PostAsync(PlayerUri, payload.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonDocument> BrowseAsync(string browseId, CancellationToken cancellationToken)
    {
        var context = await CreateContextAsync(cancellationToken).ConfigureAwait(false);
        using var payload = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("context");
            context.WriteTo(writer);
            writer.WriteString("browseId", browseId);
            writer.WriteEndObject();
        }

        return await PostAsync(BrowseUri, payload.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<JsonElement> CreateContextAsync(CancellationToken cancellationToken)
    {
        await EnsureBootstrappedAsync(cancellationToken).ConfigureAwait(false);

        using var buffer = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteStartObject("client");
            writer.WriteString("clientName", _options.ClientName);
            writer.WriteString("clientVersion", _clientVersion ?? FallbackClientVersion);
            writer.WriteString("hl", _options.Language);
            writer.WriteString("gl", _options.Region);
            if (!string.IsNullOrWhiteSpace(_visitorData))
            {
                writer.WriteString("visitorData", _visitorData);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        using var doc = JsonDocument.Parse(buffer.ToArray());
        return doc.RootElement.Clone();
    }

    private async Task EnsureBootstrappedAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_clientVersion) && !string.IsNullOrWhiteSpace(_apiKey))
        {
            return;
        }

        await _bootstrapLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrWhiteSpace(_clientVersion) && !string.IsNullOrWhiteSpace(_apiKey))
            {
                return;
            }

            _apiKey = string.IsNullOrWhiteSpace(_options.ApiKey) ? DefaultApiKey : _options.ApiKey;
            _clientVersion = string.IsNullOrWhiteSpace(_options.ClientVersion) ? null : _options.ClientVersion;
            _visitorData = _options.VisitorData;

            if (!string.IsNullOrWhiteSpace(_clientVersion))
            {
                return;
            }

            try
            {
                using var response = await _http.GetAsync(HomeUri, cancellationToken).ConfigureAwait(false);
                var html = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _clientVersion = FirstGroup(html, """INNERTUBE_CLIENT_VERSION":"([^"]+)""") ?? FallbackClientVersion;
                _apiKey = FirstGroup(html, """INNERTUBE_API_KEY":"([^"]+)""") ?? _apiKey;
                _visitorData ??= FirstGroup(html, """VISITOR_DATA":"([^"]+)""");
                _logger.LogDebug("Bootstrapped YouTube Music client version {Version}", _clientVersion);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Failed to bootstrap YouTube Music client version; using fallback {Version}", FallbackClientVersion);
                _clientVersion = FallbackClientVersion;
            }
        }
        finally
        {
            _bootstrapLock.Release();
        }
    }

    private async Task<JsonDocument> PostAsync(Uri uri, byte[] payload, CancellationToken cancellationToken)
    {
        await EnsureBootstrappedAsync(cancellationToken).ConfigureAwait(false);

        var builder = new UriBuilder(uri);
        var key = _apiKey ?? DefaultApiKey;
        builder.Query = string.IsNullOrEmpty(builder.Query)
            ? $"key={Uri.EscapeDataString(key)}"
            : $"{builder.Query.TrimStart('?')}&key={Uri.EscapeDataString(key)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, builder.Uri)
        {
            Content = new ByteArrayContent(payload)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = Encoding.UTF8.WebName
        };
        request.Headers.TryAddWithoutValidation("X-YouTube-Client-Name", "67");
        request.Headers.TryAddWithoutValidation("X-YouTube-Client-Version", _clientVersion ?? FallbackClientVersion);

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var snippet = Encoding.UTF8.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, 300)));
            throw new HttpRequestException(
                $"YouTube Music request to {uri.AbsolutePath} failed with {(int)response.StatusCode} {response.StatusCode}: {snippet}",
                inner: null,
                statusCode: response.StatusCode);
        }

        if (bytes.Length == 0)
        {
            throw new InvalidOperationException($"YouTube Music returned an empty body from {uri.AbsolutePath}.");
        }

        return JsonDocument.Parse(bytes);
    }

    private static string? FirstGroup(string input, string pattern)
    {
        var match = Regex.Match(input, pattern);
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }
}
