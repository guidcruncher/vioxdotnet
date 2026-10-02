using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Core.Configuration;
using Viox.Core.Playlists;
using Viox.Core.Plugins;
using Viox.Core.Services;
using Viox.Core.Storage;

namespace Viox.Core.Extensions;

/// <summary>
/// Unified dependency injection extensions for Viox Core services.
/// </summary>
public static class VioxCoreServiceCollectionExtension
{
    /// <summary>
    /// Registers all Viox Core services, configuration options, and plugin modules in the correct order.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddVioxCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMemoryCache();
        services.AddServerEventPublisher(configuration);
        services.AddUserAgentProvider(configuration);
        services.AddVioxPlaylists(configuration);
        services.AddClientOptionsServices(configuration);
        services.AddCurrentMediaService(configuration);
        services.AddAlsaAudioControls(configuration);
        services.AddEqPresetServices(configuration);
        services.AddHtmlSanitizer();
        services.AddFavoritesEngine(configuration);
        services.AddMediaResolver(configuration);
        services.AddMediaSearchServices(configuration);
        services.AddStaticPluginModules(configuration);
        services.AddDynamicPluginModules(configuration);

        return services;
    }

    /// <summary>
    /// Adds HTML sanitizer service to the DI container.
    /// </summary>
    public static IServiceCollection AddHtmlSanitizer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
        return services;
    }

    /// <summary>
    /// Adds ALSA equalizer and mixer services to the DI container.
    /// </summary>
    public static IServiceCollection AddAlsaAudioControls(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AlsaControlOptions>(configuration.GetSection(AlsaControlOptions.SectionName));
        services.AddSingleton<IAlsaProcessExecutor, AlsaProcessExecutor>();
        services.AddTransient<IAlsaEqualizerService, AlsaEqualizerService>();

        return services;
    }

    /// <summary>
    /// Adds playlist management services and options to the DI container.
    /// </summary>
    public static IServiceCollection AddVioxPlaylists(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<PlaylistOptions>(configuration.GetSection(PlaylistOptions.SectionName));
        services.AddSingleton<IPlaylistRepository, FileSystemPlaylistRepository>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        return services;
    }

    /// <summary>
    /// Adds client configuration options and store services to the DI container.
    /// </summary>
    public static IServiceCollection AddClientOptionsServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ClientOptionsStoreOptions>(configuration.GetSection(ClientOptionsStoreOptions.SectionName));
        services.AddSingleton<IClientOptionsStore, JsonClientOptionsStore>();

        return services;
    }

    /// <summary>
    /// Adds the global current media tracking singleton to the service collection.
    /// </summary>
    public static IServiceCollection AddCurrentMediaService(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configuration is not null)
        {
            services.Configure<CurrentMediaServiceOptions>(configuration.GetSection(CurrentMediaServiceOptions.SectionName));
        }
        else
        {
            services.Configure<CurrentMediaServiceOptions>(_ => { });
        }

        services.AddSingleton<ICurrentMediaService, CurrentMediaService>();

        return services;
    }

    /// <summary>
    /// Adds equalizer preset loader services to the DI container.
    /// </summary>
    public static IServiceCollection AddEqPresetServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<EqPresetOptions>(configuration.GetSection(EqPresetOptions.SectionName));
        services.AddScoped<IEqPresetLoader, EqPresetLoader>();

        return services;
    }

    /// <summary>
    /// Adds Favorites Storage Engine and application services to the DI container.
    /// </summary>
    public static IServiceCollection AddFavoritesEngine(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<FavoritesStorageOptions>(configuration.GetSection(FavoritesStorageOptions.SectionName));
        services.AddSingleton<IFavoritesStorageEngine, FavoritesStorageEngine>();
        services.AddSingleton<IFavoritesService, FavoritesService>();

        return services;
    }

    /// <summary>
    /// Adds Favorites Storage Engine with custom explicit options configured via delegate.
    /// </summary>
    public static IServiceCollection AddFavoritesEngine(this IServiceCollection services, Action<FavoritesStorageOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IFavoritesStorageEngine, FavoritesStorageEngine>();
        services.AddScoped<IFavoritesService, FavoritesService>();

        return services;
    }

    /// <summary>
    /// Adds media resolution converters and services to the DI container.
    /// </summary>
    public static IServiceCollection AddMediaResolver(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<MediaMetaDataConverterResolver>();
        services.AddSingleton<MediaSourceResolverService>();

        return services;
    }

    /// <summary>
    /// Adds media search services and options to the DI container.
    /// </summary>
    public static IServiceCollection AddMediaSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MediaSearchOptions>(configuration.GetSection(MediaSearchOptions.SectionName));
        services.AddSingleton<MediaSearchService>();

        return services;
    }

    /// <summary>
    /// Adds memory cache service implementation using configuration.
    /// </summary>
    public static IServiceCollection AddMemoryCacheService<TValue>(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMemoryCache();
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.AddSingleton<IMemoryCacheService<TValue>, MemoryCacheService<TValue>>();

        return services;
    }

    /// <summary>
    /// Adds memory cache service implementation using explicit options delegate.
    /// </summary>
    public static IServiceCollection AddMemoryCacheService<TValue>(this IServiceCollection services, Action<CacheOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddMemoryCache();
        services.Configure(configureOptions);
        services.AddSingleton<IMemoryCacheService<TValue>, MemoryCacheService<TValue>>();

        return services;
    }

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
        ILogger logger = loggerFactory.CreateLogger(typeof(VioxCoreServiceCollectionExtension));

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

    /// <summary>
    /// Discovers and registers compile-time plugin modules into the service collection.
    /// </summary>
    public static IServiceCollection AddStaticPluginModules(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PluginLoaderConfiguration>(configuration.GetSection(PluginLoaderConfiguration.SectionName));

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger(typeof(VioxCoreServiceCollectionExtension));

        logger.LogInformation("Starting discovery of compile-time plugin modules...");

        HashSet<Assembly> assemblies = DiscoverAllCompiledAssemblies();
        List<IPluginModule> discoveredModules = [];

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

    /// <summary>
    /// Registers the <see cref="UserAgentProvider"/> as a Singleton to ensure the selected User-Agent stays identical for the application's lifecycle.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration section to bind options from (optional).</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserAgentProvider(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<UserAgentOptions>();
        if (configuration is not null)
        {
            services.Configure<UserAgentOptions>(
                configuration.GetSection(UserAgentOptions.SectionName));
        }
        services.AddSingleton<IUserAgentProvider, UserAgentProvider>();
        return services;
    }

    public static IServiceCollection AddServerEventPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServerEventOptions>(
            configuration.GetSection(ServerEventOptions.SectionName));
        services.AddSingleton<IServerEventPublisher, ServerEventPublisher>();
        services.AddTransient<IMediaEventService, MediaEventService>();
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
