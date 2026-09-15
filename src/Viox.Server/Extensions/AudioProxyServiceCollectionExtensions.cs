// AudioProxyServiceCollectionExtensions.cs
namespace Viox.Server.Extensions;

using System;
using System.Net;
using System.Net.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Server.Configuration;
using Viox.Server.Services;

public static class AudioProxyServiceCollectionExtensions
{
    public static IServiceCollection AddAudioProxyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Framework Services
        services.AddControllers();

        // 2. Options Pattern Configuration
        services.Configure<StreamingOptions>(
            configuration.GetSection(StreamingOptions.SectionName));

        // 3. Explicit Microsoft Logging Extensions setup
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        // 4. Cache & Background Cleanup Services
        services.AddSingleton<AudioCacheManager>();
        services.AddHostedService<AudioCacheCleanupService>();

        // 5. Named HttpClient Registration targeting .NET 10 using SocketsHttpHandler
        services.AddHttpClient("AudioProxyClient", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<StreamingOptions>>().Value;
            client.Timeout = TimeSpan.FromMinutes(options.TimeoutMinutes);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = DecompressionMethods.None
        });

        return services;
    }
}
