namespace Viox.Server.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Cronos;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Scheduler;

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
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public void AddUserTask(IScheduledTask task)
    {
        var cronExpression = CronExpression.Parse(task.Schedule, CronFormat.IncludeSeconds);
        _scheduledTasks.Add(new SchedulerTaskRegistration(task, cronExpression));
        _logger.LogInformation("Added user task: {TaskName} with schedule {Schedule}", task.Name, task.Schedule);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Viox Scheduler is disabled via configuration.");
            await base.StartAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Initializing scheduled tasks discovery via Dependency Injection and file reader...");

        using (var scope = _serviceProvider.CreateScope())
        {
            // 1. Discover tasks from DI container
            var tasks = scope.ServiceProvider.GetServices<IScheduledTask>();
            foreach (var task in tasks)
            {
                try
                {
                    var cronStr = task.Schedule.Replace("\0", string.Empty);
                    var cronExpression = CronExpression.Parse(cronStr, CronFormat.IncludeSeconds);
                    _scheduledTasks.Add(new SchedulerTaskRegistration(task, cronExpression));
                    _logger.LogInformation("Discovered scheduled task: {TaskName} with schedule {Schedule} (RunOnStartup: {RunOnStartup})",
                        task.Name, task.Schedule, task.RunOnStartup);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse cron expression '{Schedule}' for task '{TaskName}'", task.Schedule, task.Name);
                }
            }

            // 2. Load user tasks from file/configuration reader and register using AddUserTask
            try
            {
                var fileReader = scope.ServiceProvider.GetService<IUserTaskSettingsFileReader>();
                if (fileReader != null)
                {
                    var settingsList = await fileReader.ReadSettingsAsync(cancellationToken);
                    foreach (var setting in settingsList)
                    {
                        IScheduledTask task = UserTask.CreateUserTask(scope, setting);
                        AddUserTask(task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user task settings from file reader.");
            }
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }
        _logger.LogInformation("Cron background task scheduler started.");

        foreach (var taskRegistration in _scheduledTasks)
        {
            if (taskRegistration.Task.RunOnStartup)
            {
                _logger.LogInformation("Triggering startup execution for task: {TaskName}", taskRegistration.Task.Name);
                _ = ExecuteTaskSafelyAsync(taskRegistration.Task, stoppingToken);
            }
        }

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
        if (!taskRegistrationState.GetOrAdd(task.Name, _ => new SemaphoreSlim(1, 1)).Wait(0))
        {
            _logger.LogWarning("Task {TaskName} is already running. Skipping this execution cycle to avoid overlap.", task.Name);
            return;
        }
        try
        {
            using var scope = _serviceProvider.CreateScope();
            _logger.LogInformation("Starting execution of task: {TaskName}", task.Name);
            await task.ExecuteAsync(cancellationToken);
            _logger.LogInformation("Successfully completed execution of task: {TaskName}", task.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while executing task: {TaskName}", task.Name);
        }
        finally
        {
            taskRegistrationState[task.Name].Release();
        }
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> taskRegistrationState = new();
    private sealed record SchedulerTaskRegistration(IScheduledTask Task, CronExpression CronExpression);
}
