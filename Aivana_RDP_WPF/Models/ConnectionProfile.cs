using System.ComponentModel.DataAnnotations;
using Aivana_RDP_WPF.Infrastructure.Protocols;

namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a connection profile for RDP connections.
/// </summary>
public class ConnectionProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ServerAddress { get; set; } = string.Empty;

    public int Port { get; set; } = 3389;

    [MaxLength(100)]
    public string? Username { get; set; }

    [MaxLength(100)]
    public string? Domain { get; set; }

    public bool IsFavorite { get; set; }

    [MaxLength(100)]
    public string? GroupName { get; set; }

    /// <summary>
    /// Tags stored as JSON array string.
    /// </summary>
    public string Tags { get; set; } = "[]";

    /// <summary>
    /// Settings stored as JSON object string.
    /// </summary>
    public string Settings { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastConnectedAt { get; set; }

    public int ConnectionCount { get; set; } = 0;

    // Multi-Protocol Support Properties
    /// <summary>
    /// The protocol type for this connection (RDP, SSH, VNC)
    /// </summary>
    public ProtocolType ProtocolType { get; set; } = ProtocolType.RDP;

    /// <summary>
    /// Protocol-specific settings stored as JSON
    /// </summary>
    public string? ProtocolSpecificSettings { get; set; }

    // Wake-on-LAN Properties
    /// <summary>
    /// MAC address for Wake-on-LAN functionality
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// Whether Wake-on-LAN is enabled for this profile
    /// </summary>
    public bool EnableWakeOnLan { get; set; }

    /// <summary>
    /// Timeout in seconds to wait for host to wake up
    /// </summary>
    public int WakeTimeoutSeconds { get; set; } = 30;

    // SSH Tunnel Properties
    /// <summary>
    /// Whether to use SSH tunnel for this connection
    /// </summary>
    public bool UseSshTunnel { get; set; }

    /// <summary>
    /// SSH tunnel configuration stored as JSON
    /// </summary>
    public string? SshTunnelConfig { get; set; }

    // Navigation property
    public virtual ICollection<SessionHistory> SessionHistories { get; set; } = new List<SessionHistory>();
}

