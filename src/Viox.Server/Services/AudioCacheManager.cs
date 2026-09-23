namespace Viox.Server.Services;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Server.Configuration;

public class AudioCacheManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<StreamingOptions> _streamingOptions;
    private readonly ILogger<AudioCacheManager> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _downloadLocks = new();
    private readonly string _cacheDirectory;

    public AudioCacheManager(
        IHttpClientFactory httpClientFactory,
        IOptions<StreamingOptions> streamingOptions,
        ILogger<AudioCacheManager> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _streamingOptions = streamingOptions ?? throw new ArgumentNullException(nameof(streamingOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheDirectory = "/music/cache";
        Directory.CreateDirectory(_cacheDirectory);
    }

    public string GetCacheDirectory() => _cacheDirectory;

    public async Task<string?> GetOrDownloadAudioAsync(string sourceUrl, DateTimeOffset? created, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceUrl);

        string fileHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sourceUrl)));
        string filePath = Path.Combine(_cacheDirectory, $"{fileHash}.mp3");

        if (File.Exists(filePath))
        {
            _logger.LogDebug("Cache hit for audio URL hash: {Hash}", fileHash);
            File.SetLastAccessTimeUtc(filePath, DateTime.UtcNow);
            return filePath;
        }

        SemaphoreSlim myLock = _downloadLocks.GetOrAdd(fileHash, _ => new SemaphoreSlim(1, 1));
        await myLock.WaitAsync(cancellationToken);

        try
        {
            // Double check after acquiring lock
            if (File.Exists(filePath))
            {
                File.SetLastAccessTimeUtc(filePath, DateTime.UtcNow);
                return filePath;
            }

            _logger.LogInformation("Cache miss. Downloading podcast audio to local file storage from {Url}", sourceUrl);
            string tempPath = Path.Combine(_cacheDirectory, $"{fileHash}.tmp");
            var client = _httpClientFactory.CreateClient("AudioProxyClient");

            using (var response = await client.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to download audio payload. Upstream status: {StatusCode}", response.StatusCode);
                    return null;
                }

                await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
                {
                    await response.Content.CopyToAsync(fileStream, cancellationToken);
                }
            }

            File.Move(tempPath, filePath, overwrite: true);
            File.SetLastAccessTimeUtc(filePath, DateTime.UtcNow);

            if (created.HasValue)
            {
                File.SetCreationTimeUtc(filePath, created.Value.UtcDateTime);
            }

            _logger.LogInformation("Successfully cached audio file locally: {FilePath}", filePath);
            return filePath;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Download cancelled for audio URL: {Url}", sourceUrl);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while caching podcast file for URL: {Url}", sourceUrl);
            return null;
        }
        finally
        {
            myLock.Release();
            if (myLock.CurrentCount == 1)
            {
                _downloadLocks.TryRemove(fileHash, out _);
            }
        }
    }
}
