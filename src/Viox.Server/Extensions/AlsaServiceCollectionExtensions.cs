using Viox.Server.Configuration;
using Viox.Server.Services;

namespace Viox.Server.Extensions;

/// <summary>
/// Dependency Injection extensions for ALSA control services.
/// </summary>
public static class AlsaServiceCollectionExtensions
{
    /// <summary>
    /// Adds ALSA equalizer and mixer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration tree to bind options from.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddAlsaAudioControls(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AlsaControlOptions>(configuration.GetSection(AlsaControlOptions.SectionName));

        services.AddSingleton<IAlsaProcessExecutor, AlsaProcessExecutor>();
        services.AddTransient<IAlsaEqualizerService, AlsaEqualizerService>();

        return services;
    }
}
