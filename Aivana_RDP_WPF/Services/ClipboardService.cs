using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for clipboard synchronization.
/// </summary>
public class ClipboardService : IClipboardService
{
    private readonly ILogger<ClipboardService> _logger;

    public ClipboardService(ILogger<ClipboardService> logger)
    {
        _logger = logger;
    }
}

