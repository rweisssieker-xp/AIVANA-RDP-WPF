namespace Aivana_RDP_WPF.Infrastructure.Protocols;

/// <summary>
/// Capabilities and features supported by a protocol
/// </summary>
public class ProtocolCapabilities
{
    public bool SupportsFileTransfer { get; set; }
    public bool SupportsClipboardSync { get; set; }
    public bool SupportsPrinterRedirection { get; set; }
    public bool SupportsAudioRedirection { get; set; }
    public int MaxColorDepth { get; set; }
    public bool SupportsMultiMonitor { get; set; }
    public List<string> SupportedAuthMethods { get; set; } = new();
    
    public override string ToString()
    {
        var features = new List<string>();
        if (SupportsFileTransfer) features.Add("File Transfer");
        if (SupportsClipboardSync) features.Add("Clipboard");
        if (SupportsPrinterRedirection) features.Add("Printer");
        if (SupportsAudioRedirection) features.Add("Audio");
        if (SupportsMultiMonitor) features.Add("Multi-Monitor");
        
        return string.Join(", ", features) + $" (Max Depth: {MaxColorDepth}bit)";
    }
}
