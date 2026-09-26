using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Viox.Client.RadioBrowser.Configuration;
using Viox.Client.RadioBrowser.Services;
using Viox.Core.Services;

namespace Viox.Client.RadioBrowser.Extensions;

/// <summary>
/// Dependency-injection helpers for <see cref="RadioBrowserClient"/>.
/// </summary>
public static class RadioBrowserServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IRadioBrowserClient"/> as a typed HTTP client.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="configure">Optional callback that customizes <see cref="RadioBrowserOptions"/>.</param>
    /// <returns>The same service collection.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddRadioBrowserClient(options =>
    /// {
    ///     options.UserAgent = "MyRadioApp/1.0";
    ///     options.BaseAddress = new Uri("https://de1.api.radio-browser.info");
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddRadioBrowserClient(
        this IServiceCollection services,
        Action<RadioBrowserOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<RadioBrowserOptions>();
        }

        services.AddHttpClient<IRadioBrowserClient, RadioBrowserClient>((httpClient, provider) =>
            {
                var options = provider.GetRequiredService<IOptions<RadioBrowserOptions>>().Value;
                httpClient.BaseAddress = options.BaseAddress ?? options.FallbackBaseAddress;
                httpClient.Timeout = options.Timeout;
                return new RadioBrowserClient(httpClient, options);
            });

        // Register implementations with unique string keys using Microsoft DI extensions
        services.AddKeyedSingleton<IMediaSource, RadioBrowserMediaSource>("radiobrowser");


        return services;
    }
}
