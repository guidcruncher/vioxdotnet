namespace Viox.Server.Logging;

public class VioxLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ILogger _innerLogger;

    public VioxLogger(string categoryName, ILogger innerLogger)
    {
        _categoryName = categoryName;
        _innerLogger = innerLogger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _innerLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Custom timestamp format: yyyy-MM-dd HH:mm:ss.fff
        // Example: 2026-10-13 04-24-03.829
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // Use the inner logger to output the formatted message with the prepended timestamp
        _innerLogger.Log(logLevel, eventId, state, exception, (s, e) =>
        {
            string originalMessage = formatter(s, e);
            return $"{timestamp} {originalMessage}";
        });
    }
}
