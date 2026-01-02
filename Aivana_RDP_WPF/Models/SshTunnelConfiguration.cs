namespace Aivana_RDP_WPF.Models;

/// <summary>
/// SSH tunnel configuration
/// </summary>
public class SshTunnelConfiguration 
{
    public string SshHost { get; set; } = string.Empty;
    public int SshPort { get; set; } = 22;
    public string SshUser { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string LocalHost { get; set; } = "localhost";
    public int LocalPort { get; set; }
    public string RemoteHost { get; set; } = string.Empty;
    public int RemotePort { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Result of SSH tunnel operation
/// </summary>
public class TunnelResult 
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TunnelId { get; set; } = string.Empty;
    public int LocalPort { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static TunnelResult Successful(string tunnelId, int localPort, string message = "SSH tunnel established")
    {
        return new TunnelResult
        {
            Success = true,
            Message = message,
            TunnelId = tunnelId,
            LocalPort = localPort,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static TunnelResult Failed(string message)
    {
        return new TunnelResult
        {
            Success = false,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Information about an active SSH tunnel
/// </summary>
public class TunnelInfo 
{
    public string TunnelId { get; set; } = string.Empty;
    public SshTunnelConfiguration Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
