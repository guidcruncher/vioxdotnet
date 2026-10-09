namespace Viox.Epg.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Epg.Configuration;

public class EpgBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EpgOptions _options;
    private readonly ILogger<EpgBackgroundService> _logger;

    private const int TargetHour = 4; // 4:00 AM

    public EpgBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<EpgOptions> options,
        ILogger<EpgBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.RunOnStartup)
        {
            _logger.LogInformation("EpgBackgroundService running startup check...");
            try
            {
                await RunImportIfNeededAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during startup EPG import check.");
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = GetDelayUntilNext4Am();
                _logger.LogInformation("EpgBackgroundService next scheduled import run in {Hours:F2} hours (at 04:00 AM).", delay.TotalHours);

                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("EpgBackgroundService running scheduled 04:00 AM EPG check...");
                await RunImportIfNeededAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful cancellation during shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during scheduled background EPG import.");
                // Wait a short duration before retrying if an error occurred
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }

    private static TimeSpan GetDelayUntilNext4Am()
    {
        var now = DateTime.UtcNow;
        var nextRun = new DateTime(now.Year, now.Month, now.Day, TargetHour, 0, 0, DateTimeKind.Utc);

        if (now >= nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun - now;
    }

    private async Task RunImportIfNeededAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<EpgImportService>();

        if (await importService.HasRunTodayAsync(cancellationToken))
        {
            _logger.LogInformation("EPG import has already successfully run today. Skipping import.");
            return;
        }

        await importService.ImportAsync(cancellationToken);
    }
}
