namespace Viox.Server.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up API services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers API services, controllers, and options with the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application <see cref="IConfiguration"/> instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers();

        // Register OpenAPI generator using passed configuration parameter
        services.AddOpenApi(configuration);

        return services;
    }
}
