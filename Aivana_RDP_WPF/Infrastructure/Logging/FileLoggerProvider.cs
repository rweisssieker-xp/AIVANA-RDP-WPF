using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aivana_RDP_WPF.Infrastructure.Logging;

/// <summary>
/// Simple file logger provider for writing logs to files.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly string _logDirectory;

    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        _options = options.Value;
        _logDirectory = _options.LogDirectory;

        // Ensure log directory exists
        if (!string.IsNullOrEmpty(_logDirectory) && !Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logDirectory, _options);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}

