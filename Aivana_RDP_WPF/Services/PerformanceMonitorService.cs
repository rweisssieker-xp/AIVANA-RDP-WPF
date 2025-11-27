using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for performance monitoring.
/// </summary>
public class PerformanceMonitorService : IPerformanceMonitorService
{
    private readonly ILogger<PerformanceMonitorService> _logger;

    public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger)
    {
        _logger = logger;
    }
}

