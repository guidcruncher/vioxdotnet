using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;

namespace Viox.Core.Extensions;

public static class ServerEventServiceCollectionExtensions
{
    public static IServiceCollection AddServerEventPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServerEventOptions>(
            configuration.GetSection(ServerEventOptions.SectionName));

        services.AddSingleton<IServerEventPublisher, ServerEventPublisher>();
        services.AddTransient<IMediaEventService, MediaEventService>();

        return services;
    }
}
