using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Infrastructure.Logging;

/// <summary>
/// Options for file logger.
/// </summary>
public class FileLoggerOptions
{
    public string LogDirectory { get; set; } = string.Empty;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    public int RetentionDays { get; set; } = 30;
}

