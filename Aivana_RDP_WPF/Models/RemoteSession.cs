namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a remote session
/// </summary>
public class RemoteSession 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ConnectionProfile Profile { get; set; } = new();
    public SessionStatus Status { get; set; } = SessionStatus.Connecting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public TimeSpan? Duration => ConnectedAt.HasValue ? 
        (DisconnectedAt ?? DateTime.UtcNow) - ConnectedAt.Value : null;
    public PerformanceMetrics? CurrentMetrics { get; set; }
    public string WindowHandle { get; set; } = string.Empty;
    public SessionState State { get; set; } = new();
    public List<SessionEvent> Events { get; set; } = new();
}

/// <summary>
/// Status of a remote session
/// </summary>
public enum SessionStatus 
{
    Connecting,
    Connected,
    Disconnected,
    Suspended,
    Error,
    Reconnecting
}

/// <summary>
/// State information for a session
/// </summary>
public class SessionState 
{
    public bool IsFullScreen { get; set; }
    public double ZoomLevel { get; set; } = 1.0;
    public int Width { get; set; }
    public int Height { get; set; }
    public string ClipboardContent { get; set; } = string.Empty;
    public bool AudioRedirected { get; set; }
    public bool PrinterRedirected { get; set; }
    public bool ClipboardRedirected { get; set; }
}

/// <summary>
/// Event that occurred during a session
/// </summary>
public class SessionEvent 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SessionEventType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Types of session events
/// </summary>
public enum SessionEventType 
{
    Connected,
    Disconnected,
    Error,
    Warning,
    Info,
    FileTransferStarted,
    FileTransferCompleted,
    ClipboardUpdated,
    SettingsChanged
}
