using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Podverse.Configuration;
using Viox.Client.Podverse.Services;

namespace Viox.Client.Podverse.Extensions;

/// <summary>
/// Dependency Injection setup extensions for registering the PodcastEpisodeParser service.
/// </summary>
public static class PodcastEpisodeParserExtensions
{
    public static IServiceCollection AddPodcastEpisodeParser(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PodcastEpisodeParserOptions>(configuration.GetSection(PodcastEpisodeParserOptions.SectionName));
        services.AddTransient<PodcastEpisodeParser>();

        return services;
    }
}
