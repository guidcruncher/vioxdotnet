using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using Viox.Server.Logging;

namespace Viox.Server.Extensions;

public static class VioxLoggingExtensions
{
    public static ILoggingBuilder AddVioxLogger(this ILoggingBuilder builder)
    {
        builder.AddConsole();
        builder.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
        builder.AddFilter("Microsoft.Extensions.Http", LogLevel.None);

        builder.Services.AddSingleton<ILoggerProvider, VioxLoggerProvider>(serviceProvider =>
        {
            // ConsoleLoggerProvider expects an IOptionsMonitor<ConsoleLoggerOptions>
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>();
            var consoleProvider = new ConsoleLoggerProvider(optionsMonitor);

            return new VioxLoggerProvider(consoleProvider);
        });

        return builder;
    }
}
