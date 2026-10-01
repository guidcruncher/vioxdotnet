using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Youtube.Configuration;
using Viox.Client.Youtube.Services;

namespace Viox.Client.Youtube.Extensions;

/// <summary>
/// Provides extension methods for registering yt-dlp and MPD integration services into the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the yt-dlp stream extraction and MPD control services along with configuration options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddYtDlpMpdIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<YtDlpOptions>(configuration.GetSection(YtDlpOptions.SectionName));
        services.AddTransient<IYtDlpStreamExtractor, YtDlpStreamExtractor>();

        return services;
    }
}
