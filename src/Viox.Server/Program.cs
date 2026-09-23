namespace Viox.Server;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using Viox.Client.Librespot.Extensions;
using Viox.Client.Mpd.Extensions;
using Viox.Client.Podverse.Extensions;
using Viox.Client.RadioBrowser.Extensions;
using Viox.Client.RadioBrowser.Services;
using Viox.Client.Spotify.Extensions;
using Viox.Client.TuneIn.Extensions;
using Viox.Core.Extensions;
using Viox.Server.Extensions;
using Viox.Snapcast.Extensions;

/// <summary>
/// Application entry point and bootstrapping host builder for services.
/// </summary>
public static class Program
{
    /// <summary>
    /// Configures and runs the unified  application host.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Logging configuration
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Configure Kestrel web server
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.AddServerHeader = false;
        });

        builder.Services.AddUserAgentProvider(builder.Configuration);
        builder.Services.AddMemoryCaches(builder.Configuration);
        builder.Services.AddClientOptionsServices(builder.Configuration);

        builder.Services.AddFavoritesEngine(builder.Configuration);
        builder.Services.AddMediaResolvers(builder.Configuration);
        builder.Services.AddAudioProxyServices(builder.Configuration);
        builder.Services.AddPodcastEpisodeParser(builder.Configuration);
        builder.Services.AddCurrentMediaService(builder.Configuration);

        // Audio output services
        builder.Services.AddLibrespotClient(builder.Configuration);
        builder.Services.AddSnapcastClient(builder.Configuration);

        builder.Services.AddMpdClient(options =>
        {
            options.Host = "127.0.0.1";
            options.Port = 6600;
        });

        // Audio Source services
        builder.Services.AddPodverseClient(builder.Configuration);
        builder.Services.AddSpotifyClient(builder.Configuration);

        string radioBrowserUrl = await RadioBrowserApiServerResolver.GetFastestApiUrlAsync();
        builder.Services.AddRadioBrowserClient(options =>
            {
                options.UserAgent = "Viox.net";
                options.BaseAddress = new Uri($"https://{radioBrowserUrl}");
            });

        builder.Services.AddSpotifyClient(builder.Configuration);
        builder.Services.AddTuneInClient(builder.Configuration);
        builder.Services.AddCoreServices(builder.Configuration);
        builder.Services.AddMediaSearchServices(builder.Configuration);

        // Register APIs
        builder.Services.AddFileAuthStore(options =>
        {
            options.FilePath = "/data/auth_token.json";
            options.CreateDirectoryIfNotExists = true;
        });

        builder.Services.AddSpotifyAuthServices(builder.Configuration);
        builder.Services.AddUnifiedMediaPlayerControlSurface(builder.Configuration);
        builder.Services.AddApi(builder.Configuration);

        builder.Services.AddPodcastDownloader(builder.Configuration);

        // Register CORS service conditionally for Development mode only
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DevCorsPolicy", policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        WebApplication app = builder.Build();

        // Apply CORS middleware in the request pipeline during Development mode only
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("DevCorsPolicy");
        }

        app.UseRouting();

        // Enable OpenAPI endpoints and Scalar UI in development
        app.UseOpenApi(app.Configuration);

        app.MapControllers();

        app.UseVueApp();

        // Run application host
        await app.RunAsync().ConfigureAwait(false);
    }
}
