using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Services;
using Viox.Core.Utilities;

namespace Viox.Core.Extensions;

/// <summary>
/// Dependency Injection setup extensions for registering the PodcastEpisodeParser service.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IUriGenerator, UriGenerator>();
        services.AddSingleton<MediaSourceResolverService>();

        return services;
    }
}
