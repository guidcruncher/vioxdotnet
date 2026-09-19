// SnapcastServiceCollectionExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Snapcast.Configuration;
using Viox.Snapcast.Services;

namespace Viox.Snapcast.Extensions;

/// <summary>
/// Extension methods for setting up Snapcast client services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class SnapcastServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Snapcast JSON-RPC client services, configures options bindings,
    /// and establishes a server connection on application startup.
    /// </summary>
    public static IServiceCollection AddSnapcastClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<SnapcastOptions>(configuration.GetSection(SnapcastOptions.Position));
        services.AddSingleton<ISnapcastClient, SnapcastClient>();
        services.AddHostedService<SnapcastConnectionHostedService>();

        return services;
    }

    /// <summary>
    /// Registers the Snapcast JSON-RPC client with explicit inline configuration actions,
    /// and establishes a server connection on application startup.
    /// </summary>
    public static IServiceCollection AddSnapcastClient(this IServiceCollection services, Action<SnapcastOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<ISnapcastClient, SnapcastClient>();
        services.AddHostedService<SnapcastConnectionHostedService>();

        return services;
    }
}

