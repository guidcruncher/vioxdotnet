using Viox.Server.Configuration;
using Viox.Server.Services;

namespace Viox.Server.Extensions;

public static class ClientConfigServiceExtensions
{
    public static IServiceCollection AddClientOptionsServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ClientOptionsStoreOptions>(
            configuration.GetSection(ClientOptionsStoreOptions.SectionName));

        services.AddScoped<IClientOptionsStore, JsonClientOptionsStore>();

        return services;
    }
}
