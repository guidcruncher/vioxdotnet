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

public static class CompiledPluginLoaderService
{
    public static IServiceCollection AddStaticPluginModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PluginLoaderConfiguration>(
            configuration.GetSection(PluginLoaderConfiguration.SectionName));

        // Create a dedicated LoggerFactory for startup execution logging
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger(typeof(CompiledPluginLoaderService));

        logger.LogInformation("Starting discovery of compile-time plugin modules...");

        HashSet<Assembly> assemblies = DiscoverAllCompiledAssemblies();

        // 1. Discover all plugin module instances FIRST into an array.
        // This isolates reflection/assembly scanning from DI service mutation.
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

        // 2. Configure services sequentially.
        // Each module mutates 'services' directly without active iterations wrapping the call.
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

    private static HashSet<Assembly> DiscoverAllCompiledAssemblies()
    {
        var assemblies = new HashSet<Assembly>();

        // Load currently loaded assemblies in AppDomain
        foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!loadedAssembly.IsDynamic)
            {
                assemblies.Add(loadedAssembly);
            }
        }

        // Recursively load all referenced assemblies
        Assembly? entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        if (entryAssembly is not null)
        {
            assemblies.Add(entryAssembly);
            LoadReferencedAssembliesRecursively(entryAssembly, assemblies);
        }

        // Scan binaries in application output directory
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
