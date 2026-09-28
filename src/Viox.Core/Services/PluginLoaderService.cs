namespace Viox.Core.Services;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Core.Configuration;
using Viox.Core.Plugins;

public static class PluginLoaderService
{
    public static IServiceCollection AddDynamicPluginModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PluginLoaderConfiguration>(
            configuration.GetSection(PluginLoaderConfiguration.SectionName));

        var section = configuration.GetSection(PluginLoaderConfiguration.SectionName);
        var options = section.Get<PluginLoaderConfiguration>();

        if (options is null)
        {
            options = new PluginLoaderConfiguration();
        }

        // Build temporary provider ONLY for logging
        using var intermediateProvider = services.BuildServiceProvider();
        var loggerFactory = intermediateProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger(typeof(PluginLoaderService));

        if (string.IsNullOrWhiteSpace(options.PluginFolderPath) ||
            !Directory.Exists(options.PluginFolderPath))
        {
            logger?.LogWarning("Plugin folder path '{FolderPath}' does not exist.", options.PluginFolderPath);
            return services;
        }

        string searchPattern = string.IsNullOrWhiteSpace(options.SearchPattern)
            ? "*.dll"
            : options.SearchPattern;

        string[] dllFiles = Directory.GetFiles(
            options.PluginFolderPath,
            searchPattern,
            SearchOption.TopDirectoryOnly);

        foreach (string dllPath in dllFiles)
        {
            try
            {
                string absolutePath = Path.GetFullPath(dllPath);
                var loadContext = new PluginLoadContext(absolutePath);
                Assembly assembly = loadContext.LoadFromAssemblyPath(absolutePath);

                var moduleTypes = assembly.GetTypes()
                    .Where(type =>
                        typeof(IPluginModule).IsAssignableFrom(type) &&
                        !type.IsInterface &&
                        !type.IsAbstract);

                foreach (Type moduleType in moduleTypes)
                {
                    if (Activator.CreateInstance(moduleType) is IPluginModule module)
                    {
                        module.ConfigureServices(services, configuration);

                        logger?.LogInformation(
                            "Successfully invoked module '{ModuleType}' from '{AssemblyPath}'",
                            moduleType.FullName,
                            absolutePath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Failed to load and execute dynamic plugin module from path '{DllPath}'",
                    dllPath);
            }
        }

        return services;
    }
}
