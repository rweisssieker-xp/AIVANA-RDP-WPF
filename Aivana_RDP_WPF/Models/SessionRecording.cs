namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a session recording.
/// </summary>
public class SessionRecording
{
    public int Id { get; set; }
    public int ConnectionProfileId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public RecordingStatus Status { get; set; }
    public long FileSizeBytes { get; set; }
    public int DurationSeconds => StoppedAt.HasValue 
        ? (int)(StoppedAt.Value - StartedAt).TotalSeconds 
        : (int)(DateTime.UtcNow - StartedAt).TotalSeconds;
}

public enum RecordingStatus
{
    Recording,
    Stopped,
    Paused,
    Error
}

