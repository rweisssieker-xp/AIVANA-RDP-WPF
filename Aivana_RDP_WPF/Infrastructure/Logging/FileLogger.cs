using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Infrastructure.Logging;

/// <summary>
/// Simple file logger implementation.
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logDirectory;
    private readonly FileLoggerOptions _options;

    public FileLogger(string categoryName, string logDirectory, FileLoggerOptions options)
    {
        _categoryName = categoryName;
        _logDirectory = logDirectory;
        _options = options;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _options.MinimumLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = FormatLogEntry(logLevel, _categoryName, message, exception);

        WriteToFile(logEntry);
    }

    private string FormatLogEntry(LogLevel logLevel, string category, string message, Exception? exception)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLevelStr = logLevel.ToString().ToUpper().PadRight(5);
        var entry = $"[{timestamp}] [{logLevelStr}] [{category}] {message}";

        if (exception != null)
        {
            entry += $"{Environment.NewLine}{exception}";
        }

        return entry;
    }

    private void WriteToFile(string logEntry)
    {
        try
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var fileName = $"aivana-{date}.log";
            var filePath = Path.Combine(_logDirectory, fileName);

            File.AppendAllText(filePath, logEntry + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // Silently fail if logging fails
        }
    }
}

