using System.Collections.Concurrent;

namespace Viox.Server.Logging;

public class VioxLoggerProvider : ILoggerProvider
{
    private readonly ILoggerProvider _innerProvider;
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new(StringComparer.Ordinal);

    public VioxLoggerProvider(ILoggerProvider innerProvider)
    {
        _innerProvider = innerProvider;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new VioxLogger(name, _innerProvider.CreateLogger(name)));
    }

    public void Dispose()
    {
        _innerProvider.Dispose();
        _loggers.Clear();
    }
}
