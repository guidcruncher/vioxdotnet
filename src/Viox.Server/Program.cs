namespace Viox.Server;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Viox.Server.Extensions;
using Viox.Core.Extensions;

/// <summary>
/// Application entry point and bootstrapping host builder for services.
/// </summary>
public static class Program
{
    /// <summary>
    /// Configures and runs the unified application host.
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

        // Register all application services via the new unified extension method
	builder.Services.AddVioxCoreServices(builder.Configuration);
        builder.Services.AddVioxServerServices(builder.Configuration);

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

        // Enable OpenAPI endpoints and Scalar UI in development[span_0](start_span)[span_0](end_span)
        app.UseOpenApi(app.Configuration);

        app.MapControllers();

        app.UseVueApp();

        // Run application host
        await app.RunAsync().ConfigureAwait(false);
    }
}
