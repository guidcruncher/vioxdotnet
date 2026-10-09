namespace Viox.Epg.Services;

using System.IO.Compression;

using Microsoft.Extensions.Options;

using Viox.Epg.Configuration;

public class EpgDownloader
{
    private readonly HttpClient _httpClient;
    private readonly EpgOptions _options;

    public EpgDownloader(HttpClient httpClient, IOptions<EpgOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Stream> DownloadAndDecompressAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(_options.SourceUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        if (_options.SourceUrl.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            return new GZipStream(stream, CompressionMode.Decompress);
        }

        return stream;
    }
}
