namespace Viox.Epg.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Epg.Configuration;
using Viox.Epg.Data;
using Viox.Epg.Services;

public static class EpgServiceCollectionExtensions
{
    public static IServiceCollection AddVioxEpg(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EpgOptions>(configuration.GetSection(EpgOptions.SectionName));

        services.AddTransient<EpgDatabaseInitializer>();
        services.AddHttpClient<EpgDownloader>();
        services.AddTransient<XmltvParser>();
        services.AddTransient<EpgImportService>();
        services.AddHostedService<EpgBackgroundService>();

        return services;
    }
}
