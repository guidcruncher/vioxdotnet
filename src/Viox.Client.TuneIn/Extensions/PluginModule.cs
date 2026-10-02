namespace Viox.Client.TuneIn.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.TuneIn.Configuration;
using Viox.Client.TuneIn.Services;
using Viox.Core.Plugins;
using Viox.Core.Services;

public class PluginModule : IPluginModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedSingleton<IMediaMetaDataConverterBase, TuneInStationDetailConverter>("tunein:station");
        services.AddKeyedSingleton<IMediaSource, TuneInMediaSource>("tunein");

        AddTuneInClient(services, configuration);
    }

    /// <summary>
    /// Registers the <see cref="ITuneInClient"/> client services using configuration settings.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    private IServiceCollection AddTuneInClient(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TuneInOptions>(configuration.GetSection(TuneInOptions.SectionName));

        services.AddHttpClient<ITuneInClient, TuneInClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TuneInOptions>>().Value;

            client.BaseAddress = options.BaseAddress;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);

        });

        return services;
    }
}
