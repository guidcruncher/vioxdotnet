namespace Viox.Core.Extensions;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viox.Core.Configuration;
using Viox.Core.Services;

public static class EqPresetServiceCollectionExtensions
{
    public static IServiceCollection AddEqPresetServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<EqPresetOptions>(configuration.GetSection(EqPresetOptions.SectionName));
        services.AddScoped<IEqPresetLoader, EqPresetLoader>();

        return services;
    }
}
