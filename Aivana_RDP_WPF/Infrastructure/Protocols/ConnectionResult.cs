namespace Aivana_RDP_WPF.Infrastructure.Protocols;

/// <summary>
/// Result of a connection attempt
/// </summary>
public class ConnectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTime ConnectedAt { get; set; }
    public string? ErrorCode { get; set; }
    
    public static ConnectionResult Successful(string sessionId, string message = "Connection established")
    {
        return new ConnectionResult
        {
            Success = true,
            Message = message,
            SessionId = sessionId,
            ConnectedAt = DateTime.UtcNow
        };
    }
    
    public static ConnectionResult Failed(string message, Exception? exception = null, string? errorCode = null)
    {
        return new ConnectionResult
        {
            Success = false,
            Message = message,
            Exception = exception,
            ErrorCode = errorCode,
            ConnectedAt = DateTime.UtcNow
        };
    }
}
