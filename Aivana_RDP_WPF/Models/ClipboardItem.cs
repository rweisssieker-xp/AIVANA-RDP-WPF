namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a clipboard item.
/// </summary>
public class ClipboardItem
{
    public int Id { get; set; }
    public int ConnectionProfileId { get; set; }
    public ClipboardFormat Format { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Preview { get; set; }
}

public enum ClipboardFormat
{
    Text,
    Image,
    FileList,
    Html,
    Rtf
}

