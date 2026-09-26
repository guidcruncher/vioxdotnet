using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Core.Configuration;
using Viox.Core.Services;

namespace Viox.Core.Extensions;

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

        services.AddSingleton<IClientOptionsStore, JsonClientOptionsStore>();

        return services;
    }
}
