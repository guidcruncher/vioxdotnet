namespace Viox.Core.Extensions;

using System;
using System.Collections.Generic;
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
        var options = section.Get<PluginLoaderConfiguration>() ?? new PluginLoaderConfiguration();

        // 1. Create a lightweight, dedicated LoggerFactory for startup logging
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger(typeof(PluginLoaderService));

        if (string.IsNullOrWhiteSpace(options.PluginFolderPath) ||
            !Directory.Exists(options.PluginFolderPath))
        {
            logger.LogWarning("Plugin folder path '{FolderPath}' does not exist.", options.PluginFolderPath);
            return services;
        }

        string searchPattern = string.IsNullOrWhiteSpace(options.SearchPattern)
            ? "*.dll"
            : options.SearchPattern;

        string[] dllFiles = Directory.GetFiles(
            options.PluginFolderPath,
            searchPattern,
            SearchOption.TopDirectoryOnly);

        // 2. Pass 1: Instantiation & Discovery
        // Discover and instantiate all modules FIRST into an array so we do not mutate
        // 'services' while actively scanning files/types.
        List<(IPluginModule Module, string DllPath)> discoveredModules = new();

        foreach (string dllPath in dllFiles)
        {
            try
            {
                string absolutePath = Path.GetFullPath(dllPath);
                var loadContext = new PluginLoadContext(absolutePath);
                Assembly assembly = loadContext.LoadFromAssemblyPath(absolutePath);

                Type[] moduleTypes = assembly.GetTypes()
                    .Where(type =>
                        typeof(IPluginModule).IsAssignableFrom(type) &&
                        !type.IsInterface &&
                        !type.IsAbstract)
                    .ToArray();

                foreach (Type moduleType in moduleTypes)
                {
                    if (Activator.CreateInstance(moduleType) is IPluginModule module)
                    {
                        discoveredModules.Add((module, absolutePath));
                        logger.LogInformation(
                            "Discovered dynamic plugin module '{ModuleType}' from '{AssemblyPath}'",
                            moduleType.FullName,
                            absolutePath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to load dynamic plugin module from path '{DllPath}'",
                    dllPath);
            }
        }

        // 3. Pass 2: Configuration
        // Execute ConfigureServices sequentially outside of any type/assembly scanning loop.
        foreach ((IPluginModule module, string absolutePath) in discoveredModules)
        {
            try
            {
                module.ConfigureServices(services, configuration);
                logger.LogInformation(
                    "Successfully configured services for dynamic module '{ModuleType}' from '{AssemblyPath}'",
                    module.GetType().FullName,
                    absolutePath);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed during ConfigureServices execution for dynamic module '{ModuleType}' from '{AssemblyPath}'",
                    module.GetType().FullName,
                    absolutePath);
            }
        }

        return services;
    }
}
