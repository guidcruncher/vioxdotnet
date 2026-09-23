using System.Text.Json;

using Microsoft.Extensions.Options;

using Viox.Server.Configuration;
using Viox.Server.Models;

namespace Viox.Server.Services;

public class JsonClientOptionsStore : IClientOptionsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ClientOptionsStoreOptions _options;
    private readonly ILogger<JsonClientOptionsStore> _logger;

    public JsonClientOptionsStore(
        IOptions<ClientOptionsStoreOptions> options,
        ILogger<JsonClientOptionsStore> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public async Task<ClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var filePath = _options.FilePath;

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Configuration file not found at '{FilePath}'. Returning default configuration.", filePath);
                return new ClientConfiguration();
            }

            await using var stream = File.OpenRead(filePath);
            var configuration = await JsonSerializer.DeserializeAsync<ClientConfiguration>(stream, SerializerOptions, cancellationToken);

            return configuration ?? new ClientConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read client configuration from '{FilePath}'.", filePath);
            throw;
        }
    }

    public async Task SaveConfigurationAsync(ClientConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var filePath = _options.FilePath;

        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInformation("Created directory structure for '{DirectoryPath}'.", directoryPath);
            }

            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, configuration, SerializerOptions, cancellationToken);

            _logger.LogInformation("Successfully saved client configuration to '{FilePath}'.", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save client configuration to '{FilePath}'.", filePath);
            throw;
        }
    }
}
