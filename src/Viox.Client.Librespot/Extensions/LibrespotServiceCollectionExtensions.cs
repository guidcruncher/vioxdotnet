using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.Librespot.Configuration;
using Viox.Client.Librespot.Services;
using Viox.Client.Librespot.WebApi;
using Viox.Core.Services;

namespace Viox.Client.Librespot.Extensions;

/// <summary>
/// Extension methods for configuring Librespot client integration in Microsoft Dependency Injection.
/// </summary>
public static class LibrespotServiceCollectionExtensions
{
    /// <summary>
    /// Adds Librespot REST and WebSocket client services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddLibrespotClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = LibrespotOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<LibrespotOptions>(configuration.GetSection(sectionName));

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, LibrespotMediaSource>("librespot");

        // Register in-memory WebSocket event state container
        services.AddSingleton<LibrespotEventState>();

        // Register background worker for persistent WebSocket stream processing
        services.AddHostedService<LibrespotWebSocketWorker>();

        services.AddHttpClient<ILibrespotRestClient, LibrespotRestClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<LibrespotOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddSingleton<ILibrespotWebSocketClient, LibrespotWebSocketClient>();

        return services;
    }

    /// <summary>
    /// Adds Librespot REST and WebSocket client services using an inline configuration action.
    /// </summary>
    public static IServiceCollection AddLibrespotClient(
        this IServiceCollection services,
        Action<LibrespotOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddHttpClient<ILibrespotRestClient, LibrespotRestClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<LibrespotOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, LibrespotMediaSource>("librespot");

        services.AddSingleton<ILibrespotWebSocketClient, LibrespotWebSocketClient>();

        return services;
    }
}
