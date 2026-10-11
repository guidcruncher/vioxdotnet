using System.Text.Json;

using Viox.Core.Scheduler;

namespace Viox.Server.Services;

public class UserTaskSettingsFileReader : IUserTaskSettingsFileReader
{
    private readonly ILogger<UserTaskSettingsFileReader> _logger;

    public UserTaskSettingsFileReader(ILogger<UserTaskSettingsFileReader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<UserTaskSettings>> ReadSettingsAsync(CancellationToken cancellationToken = default)
    {
        List<UserTaskSettings> settings = new();
        _logger.LogInformation("Reading user task settings from configuration or file source.");

        var filePath = "/data/user-schedules.json";
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            _logger.LogInformation("Reading user task settings from file path: {FilePath}", filePath);
            await using var stream = File.OpenRead(filePath);
            var fileSettings = await JsonSerializer.DeserializeAsync<List<UserTaskSettings>>(stream, cancellationToken: cancellationToken);
            if (fileSettings != null)
            {
                settings = fileSettings;
            }
        }

        _logger.LogInformation("Successfully loaded {Count} user task settings.", settings.Count);
        return settings.AsReadOnly();
    }
}
