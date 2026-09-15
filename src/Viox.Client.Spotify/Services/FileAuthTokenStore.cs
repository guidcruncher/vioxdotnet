using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

public sealed class FileAuthTokenStore : IAuthTokenStore
{
    private readonly FileAuthStoreOptions _options;
    private readonly ILogger<FileAuthTokenStore> _logger;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FileAuthTokenStore(
        IOptions<FileAuthStoreOptions> options,
        ILogger<FileAuthTokenStore> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public async Task SaveTokenAsync(TokenData tokenData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tokenData);

        string fullPath = Path.GetFullPath(_options.FilePath);

        if (_options.CreateDirectoryIfNotExists)
        {
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Created directory for token storage at {DirectoryPath}", directory);
            }
        }

        _logger.LogDebug("Persisting token data to {FilePath}", fullPath);

        using (FileStream stream = new(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await JsonSerializer.SerializeAsync(stream, tokenData, SerializerOptions, cancellationToken);
        }

        _logger.LogInformation("Token data successfully written to {FilePath}", fullPath);
    }

    public async Task<TokenData?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        string fullPath = Path.GetFullPath(_options.FilePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Token file does not exist at {FilePath}", fullPath);
            return null;
        }

        try
        {
            using (FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                TokenData? tokenData = await JsonSerializer.DeserializeAsync<TokenData>(stream, SerializerOptions, cancellationToken);
                _logger.LogDebug("Retrieved token data from {FilePath}", fullPath);
                return tokenData;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read or deserialize token file at {FilePath}", fullPath);
            throw;
        }
    }

    public Task ClearTokenAsync(CancellationToken cancellationToken = default)
    {
        string fullPath = Path.GetFullPath(_options.FilePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted token file at {FilePath}", fullPath);
        }
        else
        {
            _logger.LogDebug("Attempted to clear token, but file did not exist at {FilePath}", fullPath);
        }

        return Task.CompletedTask;
    }
}
