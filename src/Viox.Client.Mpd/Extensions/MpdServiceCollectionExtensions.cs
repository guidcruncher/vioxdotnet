using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Mpd.Configuration;
using Viox.Client.Mpd.Services;

namespace Viox.Client.Mpd.Extensions;

/// <summary>
/// Extension methods for registering <see cref="IMpdClient"/> with Microsoft Dependency Injection.
/// </summary>
public static class MpdServiceCollectionExtensions
{
    /// <summary>
    /// Adds MPD client services, registers options, and configures auto-connection on host startup.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The configuration instance containing options under the default section name.</param>
    /// <returns>The configured service collection for chaining.</returns>
    public static IServiceCollection AddMpdClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MpdOptions>(configuration.GetSection(MpdOptions.SectionName));
        services.AddSingleton<IMpdClient, MpdClient>();
        services.AddHostedService<MpdConnectionHostedService>();

        return services;
    }

    /// <summary>
    /// Adds MPD client services using inline programmatic configuration and auto-connects on host startup.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configureOptions">An action to configure the <see cref="MpdOptions"/>.</param>
    /// <returns>The configured service collection for chaining.</returns>
    public static IServiceCollection AddMpdClient(this IServiceCollection services, Action<MpdOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IMpdClient, MpdClient>();
        services.AddHostedService<MpdConnectionHostedService>();

        return services;
    }
}
