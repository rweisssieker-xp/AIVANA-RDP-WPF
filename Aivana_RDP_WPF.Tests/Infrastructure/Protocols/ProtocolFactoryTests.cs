using Aivana_RDP_WPF.Infrastructure.Protocols;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Tests.Infrastructure.Protocols;

[TestFixture]
public class ProtocolFactoryTests
{
    private ServiceProvider _serviceProvider;
    private IProtocolFactory _factory;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IProtocolFactory, ProtocolFactory>();
        services.AddTransient<RdpProtocol>();
        services.AddTransient<SshProtocol>();
        services.AddTransient<VncProtocol>();
        services.AddLogging(builder => builder.AddConsole());
        
        _serviceProvider = services.BuildServiceProvider();
        _factory = _serviceProvider.GetRequiredService<IProtocolFactory>();
    }

    [Test]
    public void CreateProtocol_WithValidRdpType_ReturnsRdpProtocol()
    {
        // Act
        var protocol = _factory.CreateProtocol(ProtocolType.RDP);
        
        // Assert
        Assert.IsInstanceOf<RdpProtocol>(protocol);
        Assert.AreEqual(ProtocolType.RDP, protocol.ProtocolType);
        Assert.AreEqual("RDP", protocol.ProtocolName);
    }

    [Test]
    public void CreateProtocol_WithValidSshType_ReturnsSshProtocol()
    {
        // Act
        var protocol = _factory.CreateProtocol(ProtocolType.SSH);
        
        // Assert
        Assert.IsInstanceOf<SshProtocol>(protocol);
        Assert.AreEqual(ProtocolType.SSH, protocol.ProtocolType);
        Assert.AreEqual("SSH", protocol.ProtocolName);
    }

    [Test]
    public void CreateProtocol_WithValidVncType_ReturnsVncProtocol()
    {
        // Act
        var protocol = _factory.CreateProtocol(ProtocolType.VNC);
        
        // Assert
        Assert.IsInstanceOf<VncProtocol>(protocol);
        Assert.AreEqual(ProtocolType.VNC, protocol.ProtocolType);
        Assert.AreEqual("VNC", protocol.ProtocolName);
    }

    [Test]
    public void GetSupportedProtocols_ReturnsAllSupportedTypes()
    {
        // Act
        var protocols = _factory.GetSupportedProtocols().ToList();
        
        // Assert
        Assert.AreEqual(3, protocols.Count);
        Assert.Contains(ProtocolType.RDP, protocols);
        Assert.Contains(ProtocolType.SSH, protocols);
        Assert.Contains(ProtocolType.VNC, protocols);
    }

    [Test]
    public void GetCapabilities_WithRdpType_ReturnsRdpCapabilities()
    {
        // Act
        var capabilities = _factory.GetCapabilities(ProtocolType.RDP);
        
        // Assert
        Assert.IsTrue(capabilities.SupportsFileTransfer);
        Assert.IsTrue(capabilities.SupportsClipboardSync);
        Assert.IsTrue(capabilities.SupportsPrinterRedirection);
        Assert.IsTrue(capabilities.SupportsAudioRedirection);
        Assert.IsTrue(capabilities.SupportsMultiMonitor);
        Assert.AreEqual(32, capabilities.MaxColorDepth);
        Assert.Contains("password", capabilities.SupportedAuthMethods);
    }

    [Test]
    public void GetCapabilities_WithSshType_ReturnsSshCapabilities()
    {
        // Act
        var capabilities = _factory.GetCapabilities(ProtocolType.SSH);
        
        // Assert
        Assert.IsTrue(capabilities.SupportsFileTransfer);
        Assert.IsFalse(capabilities.SupportsClipboardSync);
        Assert.IsFalse(capabilities.SupportsPrinterRedirection);
        Assert.IsFalse(capabilities.SupportsAudioRedirection);
        Assert.IsFalse(capabilities.SupportsMultiMonitor);
        Assert.AreEqual(24, capabilities.MaxColorDepth);
        Assert.Contains("password", capabilities.SupportedAuthMethods);
        Assert.Contains("publickey", capabilities.SupportedAuthMethods);
    }

    [Test]
    public void GetCapabilities_WithVncType_ReturnsVncCapabilities()
    {
        // Act
        var capabilities = _factory.GetCapabilities(ProtocolType.VNC);
        
        // Assert
        Assert.IsFalse(capabilities.SupportsFileTransfer);
        Assert.IsTrue(capabilities.SupportsClipboardSync);
        Assert.IsFalse(capabilities.SupportsPrinterRedirection);
        Assert.IsFalse(capabilities.SupportsAudioRedirection);
        Assert.IsTrue(capabilities.SupportsMultiMonitor);
        Assert.AreEqual(32, capabilities.MaxColorDepth);
        Assert.Contains("password", capabilities.SupportedAuthMethods);
    }

    [Test]
    public void IsSupported_WithValidTypes_ReturnsTrue()
    {
        // Act & Assert
        Assert.IsTrue(_factory.IsSupported(ProtocolType.RDP));
        Assert.IsTrue(_factory.IsSupported(ProtocolType.SSH));
        Assert.IsTrue(_factory.IsSupported(ProtocolType.VNC));
    }

    [Test]
    public void CreateProtocol_WithUnsupportedType_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => {
            _factory.CreateProtocol((ProtocolType)999);
        });
    }

    [Test]
    public void GetCapabilities_WithUnsupportedType_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => {
            _factory.GetCapabilities((ProtocolType)999);
        });
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }
}
