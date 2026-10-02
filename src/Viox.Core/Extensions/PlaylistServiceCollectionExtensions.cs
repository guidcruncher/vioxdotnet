// File: PlaylistServiceCollectionExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Playlists;

namespace Viox.Core.Extensions;

/// <summary>
/// Extension methods for setting up playlist services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class PlaylistServiceCollectionExtensions
{
    public static IServiceCollection AddVioxPlaylists(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PlaylistOptions>(configuration.GetSection(PlaylistOptions.SectionName));
        services.AddSingleton<IPlaylistRepository, FileSystemPlaylistRepository>();
        services.AddTransient<IPlaylistService, PlaylistService>();

        return services;
    }
}
