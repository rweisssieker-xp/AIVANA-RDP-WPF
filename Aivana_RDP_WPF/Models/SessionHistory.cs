using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a session history entry for RDP connections.
/// </summary>
public class SessionHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ConnectionProfileId { get; set; }

    [ForeignKey(nameof(ConnectionProfileId))]
    public virtual ConnectionProfile ConnectionProfile { get; set; } = null!;

    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DisconnectedAt { get; set; }

    /// <summary>
    /// Duration stored as ticks (long) for SQLite compatibility.
    /// </summary>
    [Column(TypeName = "INTEGER")]
    public long DurationTicks { get; set; }

    [NotMapped]
    public TimeSpan Duration
    {
        get => TimeSpan.FromTicks(DurationTicks);
        set => DurationTicks = value.Ticks;
    }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Unknown";

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}

