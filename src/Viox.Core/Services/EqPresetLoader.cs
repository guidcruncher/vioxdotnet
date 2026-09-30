namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;

public sealed class EqPresetLoader : IEqPresetLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly IOptions<EqPresetOptions> _options;
    private readonly ILogger<EqPresetLoader> _logger;

    public EqPresetLoader(IOptions<EqPresetOptions> options, ILogger<EqPresetLoader> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyDictionary<string, int[]>> LoadPresetsAsync(CancellationToken cancellationToken = default)
    {
        string filePath = _options.Value.FilePath;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogError("The EQ preset file path is not configured in options.");
            throw new InvalidOperationException("EQ preset file path is missing in options.");
        }

        if (!File.Exists(filePath))
        {
            _logger.LogError("EQ presets file not found at path: {FilePath}", filePath);
            throw new FileNotFoundException($"The specified preset file '{filePath}' was not found.", filePath);
        }

        try
        {
            _logger.LogInformation("Loading EQ presets from file: {FilePath}", filePath);
            await using FileStream stream = File.OpenRead(filePath);

            Dictionary<string, int[]>? presets = await JsonSerializer.DeserializeAsync<Dictionary<string, int[]>>(
                stream,
                SerializerOptions,
                cancellationToken);

            if (presets is null)
            {
                _logger.LogWarning("Deserialization resulted in a null dictionary from file: {FilePath}", filePath);
                return new Dictionary<string, int[]>();
            }

            _logger.LogInformation("Successfully loaded {Count} EQ presets.", presets.Count);
            return presets;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize JSON content from file: {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while reading the preset file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<int[]?> GetPresetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("GetPresetByNameAsync called with empty or null preset name.");
            return null;
        }

        IReadOnlyDictionary<string, int[]> presets = await LoadPresetsAsync(cancellationToken);

        foreach (KeyValuePair<string, int[]> pair in presets)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        _logger.LogWarning("Preset with name '{PresetName}' was not found.", name);
        return null;
    }
}
