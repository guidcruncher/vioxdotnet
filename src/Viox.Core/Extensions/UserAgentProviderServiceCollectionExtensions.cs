using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Viox.Core.Configuration;
using Viox.Core.Services;

namespace Viox.Core.Extensions;

/// <summary>
/// Extension methods for registering <see cref="UserAgentProvider"/> with the DI container.
/// </summary>
public static class UserAgentProviderServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="UserAgentProvider"/> as a Singleton to ensure the selected User-Agent stays identical for the application's lifecycle.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration section to bind options from (optional).</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserAgentProvider(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<UserAgentOptions>();

        if (configuration is not null)
        {
            services.Configure<UserAgentOptions>(
                configuration.GetSection(UserAgentOptions.SectionName));
        }

        services.TryAddSingleton<IUserAgentProvider, UserAgentProvider>();

        return services;
    }
}
