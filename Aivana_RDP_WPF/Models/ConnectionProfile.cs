using System.ComponentModel.DataAnnotations;

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

    // Navigation property
    public virtual ICollection<SessionHistory> SessionHistories { get; set; } = new List<SessionHistory>();
}

