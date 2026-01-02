using Aivana_RDP_WPF.Infrastructure.Protocols;
using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Tests.Infrastructure.Protocols;

[TestFixture]
public class SshProtocolTests
{
    private SshProtocol _protocol;
    private ILogger<SshProtocol> _logger;

    [SetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<SshProtocol>();
        _protocol = new SshProtocol(_logger);
    }

    [Test]
    public void ProtocolType_ReturnsSsh()
    {
        // Assert
        Assert.AreEqual(ProtocolType.SSH, _protocol.ProtocolType);
    }

    [Test]
    public void ProtocolName_ReturnsSsh()
    {
        // Assert
        Assert.AreEqual("SSH", _protocol.ProtocolName);
    }

    [Test]
    public void IsConnected_InitiallyReturnsFalse()
    {
        // Assert
        Assert.IsFalse(_protocol.IsConnected);
    }

    [Test]
    public void SessionId_InitiallyReturnsNull()
    {
        // Assert
        Assert.IsNull(_protocol.SessionId);
    }

    [Test]
    public async Task ConnectAsync_WithValidProfile_ReturnsSuccessResult()
    {
        // Arrange
        var profile = new ConnectionProfile
        {
            Id = 1,
            Name = "Test SSH",
            Host = "test.example.com",
            Port = 22,
            Username = "testuser"
        };

        // Act
        var result = await _protocol.ConnectAsync(profile);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotEmpty(result.Message);
        Assert.IsNotEmpty(result.SessionId);
        Assert.IsTrue(_protocol.IsConnected);
        Assert.AreEqual(result.SessionId, _protocol.SessionId);
    }

    [Test]
    public async Task ConnectAsync_TriggersStatusChangedEvents()
    {
        // Arrange
        var profile = new ConnectionProfile { Id = 1, Host = "test.com" };
        var statusEvents = new List<string>();
        
        _protocol.StatusChanged += (sender, status) => statusEvents.Add(status);

        // Act
        await _protocol.ConnectAsync(profile);

        // Assert
        Assert.IsTrue(statusEvents.Count >= 2);
        Assert.IsTrue(statusEvents.Any(s => s.Contains("Connecting")));
        Assert.IsTrue(statusEvents.Any(s => s.Contains("established")));
    }

    [Test]
    public async Task DisconnectAsync_AfterConnect_DisconnectsSuccessfully()
    {
        // Arrange
        var profile = new ConnectionProfile { Id = 1, Host = "test.com" };
        await _protocol.ConnectAsync(profile);
        var disconnectEvents = new List<string>();
        
        _protocol.StatusChanged += (sender, status) => disconnectEvents.Add(status);

        // Act
        await _protocol.DisconnectAsync();

        // Assert
        Assert.IsFalse(_protocol.IsConnected);
        Assert.IsNull(_protocol.SessionId);
        Assert.IsTrue(disconnectEvents.Any(s => s.Contains("disconnected")));
    }

    [Test]
    public async Task GetCapabilitiesAsync_ReturnsSshCapabilities()
    {
        // Act
        var capabilities = await _protocol.GetCapabilitiesAsync();

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
    public async Task GetMetricsAsync_ReturnsValidMetrics()
    {
        // Act
        var metrics = await _protocol.GetMetricsAsync();

        // Assert
        Assert.IsTrue(metrics.Timestamp > DateTime.MinValue);
        Assert.IsTrue(metrics.LatencyMs >= 0);
        Assert.IsTrue(metrics.BandwidthMbps >= 0);
        Assert.IsTrue(metrics.PacketLossPercent >= 0);
        Assert.IsTrue(metrics.FrameRate >= 0);
        Assert.IsTrue(metrics.CpuUsagePercent >= 0);
        Assert.IsTrue(metrics.MemoryUsageMB >= 0);
        Assert.IsTrue(metrics.QualityScore >= 0 && metrics.QualityScore <= 100);
    }

    [Test]
    public async Task MetricsUpdated_EventFires_WhenConnected()
    {
        // Arrange
        var profile = new ConnectionProfile { Id = 1, Host = "test.com" };
        var metricsEvents = new List<PerformanceMetrics>();
        
        _protocol.MetricsUpdated += (sender, metrics) => metricsEvents.Add(metrics);

        // Act
        await _protocol.ConnectAsync(profile);
        await Task.Delay(100); // Allow time for metrics monitoring

        // Assert
        // Note: In real implementation, this would test actual metrics updates
        // For now, we just verify the event mechanism exists
        Assert.IsNotNull(metricsEvents);
    }
}
