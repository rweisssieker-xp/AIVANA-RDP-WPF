using System;

namespace Aivana_RDP_WPF.Models;

public class HealthCheckResult
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public bool PingSuccess { get; set; }
    public long? PingRoundtripMs { get; set; }

    public bool PortOpen { get; set; }
    public int Port { get; set; }
    public long? TcpConnectMs { get; set; }

    public string? ResolvedIp { get; set; }
    public string? ErrorMessage { get; set; }

    public bool IsHealthy => PingSuccess && PortOpen;
}
