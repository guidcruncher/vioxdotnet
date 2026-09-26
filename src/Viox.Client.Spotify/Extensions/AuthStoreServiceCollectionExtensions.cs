using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Services;

namespace Viox.Client.Spotify.Extensions;

public static class AuthStoreServiceCollectionExtensions
{
    public static IServiceCollection AddFileAuthStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<FileAuthStoreOptions>(configuration.GetSection(FileAuthStoreOptions.SectionName));
        services.AddSingleton<IAuthTokenStore, FileAuthTokenStore>();

        return services;
    }

    public static IServiceCollection AddFileAuthStore(
        this IServiceCollection services,
        Action<FileAuthStoreOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IAuthTokenStore, FileAuthTokenStore>();

        return services;
    }
}
