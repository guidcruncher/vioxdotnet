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
using Viox.Core.Playlists;
using Viox.Core.Plugins;
using Viox.Core.Services;
using Viox.Core.Utilities;
using Viox.Core.Models;

/// <summary>
/// Consolidated dependency injection extensions for all Viox core services, options, and plugin modules.
/// </summary>
public static class VioxCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Viox core services, configurations, audio controls, playlists, and plugin modules to the service collection in a single call.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddVioxCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Playlist Services and Options
        services.Configure<PlaylistOptions>(configuration.GetSection(PlaylistOptions.SectionName));
        services.AddSingleton<IPlaylistRepository, FileSystemPlaylistRepository>();
        services.AddTransient<IPlaylistService, PlaylistService>();

        // 2. Server Event Services and Options
        services.Configure<ServerEventOptions>(configuration.GetSection(ServerEventOptions.SectionName));
        services.AddSingleton<IServerEventPublisher, ServerEventPublisher>();
        services.AddTransient<IMediaEventService, MediaEventService>();

        // 3. ALSA Audio Control Services and Options
        services.Configure<AlsaControlOptions>(configuration.GetSection(AlsaControlOptions.SectionName));
        services.AddSingleton<IAlsaProcessExecutor, AlsaProcessExecutor>();
        services.AddTransient<IAlsaEqualizerService, AlsaEqualizerService>();

        // 4. EQ Preset Services and Options
        services.Configure<EqPresetOptions>(configuration.GetSection(EqPresetOptions.SectionName));
        services.AddScoped<IEqPresetLoader, EqPresetLoader>();

        // 5. Media Resolver Services
        services.AddSingleton<MediaMetaDataConverterResolver>();
        services.AddSingleton<MediaSourceResolverService>();

        // 6. Core Services and HTML Sanitizer Options
        services.AddTransient<IUriGenerator, UriGenerator>();
        services.Configure<HtmlSanitizerOptions>(configuration.GetSection(HtmlSanitizerOptions.SectionName));
        services.AddTransient<IHtmlSanitizerService, HtmlSanitizerService>();

        // 7. Client Options Services
        services.Configure<ClientOptionsStoreOptions>(configuration.GetSection(ClientOptionsStoreOptions.SectionName));
        services.AddSingleton<IClientOptionsStore, JsonClientOptionsStore>();

        // 8. Static Plugin Modules Loading
        services.AddStaticPluginModulesInternal(configuration);

        // 9. Dynamic Plugin Modules Loading
        services.AddDynamicPluginModulesInternal(configuration);

        return services;
    }

    private static IServiceCollection AddStaticPluginModulesInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PluginLoaderConfiguration>(
            configuration.GetSection(PluginLoaderConfiguration.SectionName));

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger(typeof(VioxCoreServiceCollectionExtensions));

        logger.LogInformation("Starting discovery of compile-time plugin modules...");

        HashSet<Assembly> assemblies = DiscoverAllCompiledAssemblies();
        List<IPluginModule> discoveredModules = new();

        foreach (Assembly assembly in assemblies)
        {
            try
            {
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
                        discoveredModules.Add(module);
                        logger.LogInformation(
                            "Discovered plugin module '{ModuleType}' in assembly '{AssemblyName}'",
                            moduleType.FullName,
                            assembly.GetName().Name);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.LogWarning(
                    ex,
                    "Skipped unresolvable types in assembly '{AssemblyName}'.",
                    assembly.FullName);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to scan assembly '{AssemblyName}' for plugin modules.",
                    assembly.FullName);
            }
        }

        foreach (IPluginModule module in discoveredModules)
        {
            try
            {
                module.ConfigureServices(services, configuration);
                logger.LogInformation(
                    "Successfully configured services for module '{ModuleType}'",
                    module.GetType().FullName);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed during ConfigureServices execution for module '{ModuleType}'",
                    module.GetType().FullName);
            }
        }

        return services;
    }

    private static IServiceCollection AddDynamicPluginModulesInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PluginLoaderConfiguration>(
            configuration.GetSection(PluginLoaderConfiguration.SectionName));

        var section = configuration.GetSection(PluginLoaderConfiguration.SectionName);
        var options = section.Get<PluginLoaderConfiguration>() ?? new PluginLoaderConfiguration();

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger(typeof(VioxCoreServiceCollectionExtensions));

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

    private static HashSet<Assembly> DiscoverAllCompiledAssemblies()
    {
        var assemblies = new HashSet<Assembly>();

        foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!loadedAssembly.IsDynamic)
            {
                assemblies.Add(loadedAssembly);
            }
        }

        Assembly? entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        if (entryAssembly is not null)
        {
            assemblies.Add(entryAssembly);
            LoadReferencedAssembliesRecursively(entryAssembly, assemblies);
        }

        string baseDirectory = AppContext.BaseDirectory;
        if (Directory.Exists(baseDirectory))
        {
            foreach (string dllPath in Directory.GetFiles(baseDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllPath);
                    if (!assemblies.Any(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName)))
                    {
                        Assembly loaded = Assembly.Load(assemblyName);
                        assemblies.Add(loaded);
                    }
                }
                catch
                {
                    // Ignore non-managed DLLs
                }
            }
        }

        return assemblies;
    }

    private static void LoadReferencedAssembliesRecursively(Assembly assembly, HashSet<Assembly> visited)
    {
        foreach (AssemblyName referencedName in assembly.GetReferencedAssemblies())
        {
            try
            {
                Assembly loadedAssembly = Assembly.Load(referencedName);
                if (!loadedAssembly.IsDynamic && visited.Add(loadedAssembly))
                {
                    LoadReferencedAssembliesRecursively(loadedAssembly, visited);
                }
            }
            catch
            {
                // Ignore unresolvable references
            }
        }
    }
}
