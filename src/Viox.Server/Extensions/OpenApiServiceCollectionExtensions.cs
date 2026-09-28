namespace Viox.Server.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Scalar.AspNetCore;

/// <summary>
/// Extension methods for setting up OpenAPI document generation and Scalar API Reference UI.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers OpenAPI generator services targeting .NET 10 native OpenAPI generation.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">Application configuration instance.</param>
    /// <returns>The updated service collection for method chaining.</returns>
    public static IServiceCollection AddOpenApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Viox.Net";
                document.Info.Version = "v1.0";
                document.Info.Description = "The Viox API";

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the HTTP request pipeline to serve OpenAPI endpoints and Scalar API reference UI.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="configuration">Application configuration instance.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication UseOpenApi(
        this WebApplication app,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configuration);

        // Serves /openapi/v1.json
        app.MapOpenApi();

        // Serves interactive Scalar UI at /docs/v1
        app.MapScalarApiReference("docs/v1", options =>
        {
            options.WithTitle("")
                   .WithTheme(ScalarTheme.Moon)
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                   .WithOpenApiRoutePattern("/openapi/v1.json");
        });

        return app;
    }
}
