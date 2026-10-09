namespace Viox.Core.Extensions;

using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Scheduler;

public static class SchedulerServiceCollectionExtensions
{
    public static IServiceCollection AddVioxScheduler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SchedulerOptions>(configuration.GetSection(SchedulerOptions.SectionName));
        services.AddHostedService<CronBackgroundSchedulerService>();
        return services;
    }

    public static IServiceCollection AddScheduledTask<TTask>(this IServiceCollection services)
        where TTask : class, IScheduledTask
    {
        services.AddTransient<IScheduledTask, TTask>();
        return services;
    }

    public static IServiceCollection AddVioxSchedulerTasksFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var taskTypes = assembly.GetTypes()
            .Where(t => typeof(IScheduledTask).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var taskType in taskTypes)
        {
            services.AddTransient(typeof(IScheduledTask), taskType);
        }

        return services;
    }

    public static IServiceCollection AddVioxSchedulerTasksFromEntryAssembly(
        this IServiceCollection services)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            services.AddVioxSchedulerTasksFromAssembly(entryAssembly);
        }
        return services;
    }
}

