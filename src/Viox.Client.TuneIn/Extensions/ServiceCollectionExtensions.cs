using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.TuneIn.Configuration;
using Viox.Client.TuneIn.Services;
using Viox.Core.Services;

namespace Viox.Client.TuneIn.Extensions;

/// <summary>
/// Extension methods for registering <see cref="ITuneInClient"/> services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Registers the <see cref="ITuneInClient"/> client services and HTTP dependencies.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configureOptions">An optional action to configure the <see cref="TuneInOptions"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddTuneInClient(
        this IServiceCollection services,
        Action<TuneInOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<TuneInOptions>();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddHttpClient<ITuneInClient, TuneInClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TuneInOptions>>().Value;

            client.BaseAddress = options.BaseAddress;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);

        });

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, TuneInMediaSource>("tunein");

        return services;
    }

    /// <summary>
    /// Registers the <see cref="ITuneInClient"/> client services using configuration settings.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddTuneInClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TuneInOptions>(configuration.GetSection(TuneInOptions.SectionName));

        return services.AddTuneInClient();
    }
}
