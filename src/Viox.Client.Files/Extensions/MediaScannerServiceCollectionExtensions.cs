namespace Viox.Client.Files.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Viox.Client.Files.Configuration;
using Viox.Client.Files.Services;

public static class MediaScannerServiceCollectionExtensions
{
    public static IServiceCollection AddFileScanner(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MediaScannerOptions>(
            configuration.GetSection(MediaScannerOptions.SectionName));

        services.AddTransient<IFileScanner, FileScanner>();

        return services;
    }
}
