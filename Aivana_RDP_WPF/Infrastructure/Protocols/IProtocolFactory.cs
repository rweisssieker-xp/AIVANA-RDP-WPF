namespace Aivana_RDP_WPF.Infrastructure.Protocols;

/// <summary>
/// Factory for creating protocol instances
/// </summary>
public interface IProtocolFactory
{
    /// <summary>
    /// Creates a protocol instance for the specified type
    /// </summary>
    IRemoteProtocol CreateProtocol(ProtocolType type);
    
    /// <summary>
    /// Gets all supported protocol types
    /// </summary>
    IEnumerable<ProtocolType> GetSupportedProtocols();
    
    /// <summary>
    /// Gets capabilities for a specific protocol type
    /// </summary>
    ProtocolCapabilities GetCapabilities(ProtocolType type);
    
    /// <summary>
    /// Validates if a protocol type is supported
    /// </summary>
    bool IsSupported(ProtocolType type);
}
