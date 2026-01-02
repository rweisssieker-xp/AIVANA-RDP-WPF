using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Aivana_RDP_WPF.Infrastructure.Protocols;

/// <summary>
/// Factory implementation for creating protocol instances
/// </summary>
public class ProtocolFactory : IProtocolFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ProtocolType, Type> _protocolTypes = new();
    private readonly Dictionary<ProtocolType, ProtocolCapabilities> _capabilities = new();

    public ProtocolFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Register protocol types
        _protocolTypes[ProtocolType.RDP] = typeof(RdpProtocol);
        _protocolTypes[ProtocolType.SSH] = typeof(SshProtocol);
        _protocolTypes[ProtocolType.VNC] = typeof(VncProtocol);
        
        // Define protocol capabilities
        _capabilities[ProtocolType.RDP] = new ProtocolCapabilities
        {
            SupportsFileTransfer = true,
            SupportsClipboardSync = true,
            SupportsPrinterRedirection = true,
            SupportsAudioRedirection = true,
            MaxColorDepth = 32,
            SupportsMultiMonitor = true,
            SupportedAuthMethods = new() { "password", "smartcard", "certificate" }
        };
        
        _capabilities[ProtocolType.SSH] = new ProtocolCapabilities
        {
            SupportsFileTransfer = true,
            SupportsClipboardSync = false,
            SupportsPrinterRedirection = false,
            SupportsAudioRedirection = false,
            MaxColorDepth = 24,
            SupportsMultiMonitor = false,
            SupportedAuthMethods = new() { "password", "publickey", "keyboard" }
        };
        
        _capabilities[ProtocolType.VNC] = new ProtocolCapabilities
        {
            SupportsFileTransfer = false,
            SupportsClipboardSync = true,
            SupportsPrinterRedirection = false,
            SupportsAudioRedirection = false,
            MaxColorDepth = 32,
            SupportsMultiMonitor = true,
            SupportedAuthMethods = new() { "password", "none" }
        };
    }

    public IRemoteProtocol CreateProtocol(ProtocolType type)
    {
        if (!_protocolTypes.TryGetValue(type, out var protocolType))
        {
            throw new NotSupportedException($"Protocol type {type} is not supported");
        }

        try
        {
            return (IRemoteProtocol)_serviceProvider.GetRequiredService(protocolType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create protocol of type {type}", ex);
        }
    }

    public IEnumerable<ProtocolType> GetSupportedProtocols()
    {
        return _protocolTypes.Keys;
    }

    public ProtocolCapabilities GetCapabilities(ProtocolType type)
    {
        if (!_capabilities.TryGetValue(type, out var capabilities))
        {
            throw new NotSupportedException($"Protocol type {type} is not supported");
        }

        return capabilities;
    }

    public bool IsSupported(ProtocolType type)
    {
        return _protocolTypes.ContainsKey(type);
    }
}
