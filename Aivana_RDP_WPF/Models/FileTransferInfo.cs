namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a file transfer operation.
/// </summary>
public class FileTransferInfo
{
    public int Id { get; set; }
    public int ConnectionProfileId { get; set; }
    public string LocalPath { get; set; } = string.Empty;
    public string RemotePath { get; set; } = string.Empty;
    public FileTransferDirection Direction { get; set; }
    public FileTransferStatus Status { get; set; }
    public long TotalBytes { get; set; }
    public long TransferredBytes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    public double ProgressPercentage => TotalBytes > 0 ? (TransferredBytes * 100.0 / TotalBytes) : 0;
}

public enum FileTransferDirection
{
    Upload,
    Download
}

public enum FileTransferStatus
{
    Pending,
    InProgress,
    Paused,
    Completed,
    Failed,
    Cancelled
}

