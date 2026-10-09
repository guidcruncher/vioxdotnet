namespace Viox.Epg.Services;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Scheduler;
using Viox.Epg.Configuration;

public class EpgScheduledTask : IScheduledTask
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EpgOptions _options;
    private readonly ILogger<EpgScheduledTask> _logger;

    public string Name => "EPG Import Task";
    public string Schedule => "0 4 * * *"; // Daily at 04:00 AM UTC
    public bool RunOnStartup => _options.RunOnStartup;

    public EpgScheduledTask(
        IServiceScopeFactory scopeFactory,
        IOptions<EpgOptions> options,
        ILogger<EpgScheduledTask> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EpgScheduledTask running EPG import check...");
        await RunImportIfNeededAsync(cancellationToken);
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

