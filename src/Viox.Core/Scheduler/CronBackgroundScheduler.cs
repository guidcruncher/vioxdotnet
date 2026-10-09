namespace Viox.Core.Scheduler;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Cronos;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


public class CronBackgroundSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CronBackgroundSchedulerService> _logger;
    private readonly SchedulerOptions _options;
    private readonly List<SchedulerTaskRegistration> _scheduledTasks = new();

    public CronBackgroundSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<CronBackgroundSchedulerService> logger,
        IOptions<SchedulerOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Viox Scheduler is disabled via configuration.");
            return base.StartAsync(cancellationToken);
        }

        _logger.LogInformation("Initializing scheduled tasks discovery via Dependency Injection...");

        using (var scope = _serviceProvider.CreateScope())
        {
            var tasks = scope.ServiceProvider.GetServices<IScheduledTask>();
            foreach (var task in tasks)
            {
                try
                {
                    var cronExpression = CronExpression.Parse(task.Schedule, CronFormat.IncludeSeconds);
                    _scheduledTasks.Add(new SchedulerTaskRegistration(task, cronExpression));
                    _logger.LogInformation("Discovered scheduled task: {TaskName} with schedule {Schedule}", task.Name, task.Schedule);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse cron expression '{Schedule}' for task '{TaskName}'", task.Schedule, task.Name);
                }
            }
        }

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        _logger.LogInformation("Cron background task scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var taskRegistration in _scheduledTasks)
            {
                var nextOccurrence = taskRegistration.CronExpression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);
                if (nextOccurrence.HasValue)
                {
                    var delay = nextOccurrence.Value - utcNow;
                    if (delay <= TimeSpan.FromSeconds(_options.CheckIntervalSeconds))
                    {
                        _logger.LogInformation("Triggering scheduled task: {TaskName}", taskRegistration.Task.Name);
                        _ = ExecuteTaskSafelyAsync(taskRegistration.Task, stoppingToken);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.CheckIntervalSeconds), stoppingToken);
        }
    }

    private async Task ExecuteTaskSafelyAsync(IScheduledTask task, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        try
        {
            _logger.LogInformation("Starting execution of task: {TaskName}", task.Name);
            await task.ExecuteAsync(cancellationToken);
            _logger.LogInformation("Successfully completed execution of task: {TaskName}", task.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while executing task: {TaskName}", task.Name);
        }
    }

    private sealed record SchedulerTaskRegistration(IScheduledTask Task, CronExpression CronExpression);
}

