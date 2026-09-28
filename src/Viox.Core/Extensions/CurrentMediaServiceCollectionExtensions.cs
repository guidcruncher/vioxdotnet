namespace Viox.Core.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;

/// <summary>
/// Service extension methods for registering media playback services.
/// </summary>
public static class CurrentMediaServiceCollectionExtensions
{
    /// <summary>
    /// Adds the global current media tracking singleton to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration section to bind options from.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCurrentMediaService(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configuration is not null)
        {
            services.Configure<CurrentMediaServiceOptions>(configuration.GetSection(CurrentMediaServiceOptions.SectionName));
        }
        else
        {
            services.Configure<CurrentMediaServiceOptions>(_ => { });
        }

        services.AddSingleton<ICurrentMediaService, CurrentMediaService>();

        return services;
    }
}
