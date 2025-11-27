using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for session recording.
/// </summary>
public class SessionRecordingService : ISessionRecordingService
{
    private readonly ILogger<SessionRecordingService> _logger;

    public SessionRecordingService(ILogger<SessionRecordingService> logger)
    {
        _logger = logger;
    }
}

