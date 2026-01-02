using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Infrastructure.Rdp;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.Infrastructure.Protocols;

/// <summary>
/// RDP protocol implementation that wraps the existing RdpConnectionService
/// </summary>
public class RdpProtocol : IRemoteProtocol
{
    private readonly ILogger<RdpProtocol> _logger;
    private readonly IRdpConnectionService _rdpService;
    private bool _isConnected;
    private string? _sessionId;
    private RdpClientWrapper? _currentWrapper;

    public ProtocolType ProtocolType => ProtocolType.RDP;
    public string ProtocolName => "RDP";
    public bool IsConnected => _isConnected;
    public string? SessionId => _sessionId;

    public event EventHandler<PerformanceMetrics>? MetricsUpdated;
    public event EventHandler<string>? StatusChanged;

    public RdpProtocol(ILogger<RdpProtocol> logger, IRdpConnectionService rdpService)
    {
        _logger = logger;
        _rdpService = rdpService;
    }

    public async Task<ConnectionResult> ConnectAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        StatusChanged?.Invoke(this, "Connecting to RDP server...");
        
        try
        {
            _logger.LogInformation("Attempting RDP connection to {Host}:{Port}", profile.ServerAddress, profile.Port);
            
            // Create connection host using existing service
            var host = _rdpService.CreateConnectionHost(profile);
            if (host == null)
            {
                return ConnectionResult.Failed("Failed to create RDP connection host");
            }

            // Get the wrapper for metrics and status
            _currentWrapper = _rdpService.GetActiveWrapper(profile.Id);
            
            // Connect using existing service
            await _rdpService.ConnectAsync(profile);
            
            _sessionId = $"rdp_{Guid.NewGuid()}";
            _isConnected = true;
            
            StatusChanged?.Invoke(this, "RDP connection established");
            
            // Start performance monitoring
            _ = Task.Run(async () => await MonitorPerformanceAsync(profile.Id, ct), ct);
            
            return ConnectionResult.Successful(_sessionId, "RDP connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RDP connection failed to {Host}:{Port}", profile.ServerAddress, profile.Port);
            return ConnectionResult.Failed($"RDP connection failed: {ex.Message}", ex);
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (!_isConnected || _currentWrapper == null) return;
        
        StatusChanged?.Invoke(this, "Disconnecting RDP session...");
        
        try
        {
            // Use existing service to disconnect
            var profileId = _currentWrapper.ProfileId;
            _rdpService.Disconnect(profileId);
            
            _isConnected = false;
            _sessionId = null;
            _currentWrapper = null;
            
            StatusChanged?.Invoke(this, "RDP session disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RDP disconnection");
        }
        
        await Task.CompletedTask;
    }

    public async Task<ProtocolCapabilities> GetCapabilitiesAsync()
    {
        return await Task.FromResult(new ProtocolCapabilities
        {
            SupportsFileTransfer = true,
            SupportsClipboardSync = true,
            SupportsPrinterRedirection = true,
            SupportsAudioRedirection = true,
            MaxColorDepth = 32,
            SupportsMultiMonitor = true,
            SupportedAuthMethods = new() { "password", "smartcard", "certificate" }
        });
    }

    public async Task<PerformanceMetrics> GetMetricsAsync()
    {
        if (_currentWrapper == null)
        {
            return new PerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                ConnectionProfileId = 0,
                QualityScore = 0
            };
        }

        // TODO: Get real metrics from RDP wrapper
        // For now, return simulated but realistic metrics
        return await Task.FromResult(new PerformanceMetrics
        {
            Timestamp = DateTime.UtcNow,
            ConnectionProfileId = _currentWrapper.ProfileId,
            LatencyMs = 15.0 + new Random().NextDouble() * 10.0,
            BandwidthMbps = 100.0 + new Random().NextDouble() * 50.0,
            PacketLossPercent = new Random().NextDouble() * 1.0,
            FrameRate = 60.0 + new Random().NextDouble() * 30.0,
            CpuUsagePercent = 10.0 + new Random().NextDouble() * 15.0,
            MemoryUsageMB = 150.0 + new Random().NextDouble() * 50.0,
            QualityScore = 90 + new Random().Next(0, 10)
        });
    }

    private async Task MonitorPerformanceAsync(int profileId, CancellationToken ct)
    {
        while (_isConnected && !ct.IsCancellationRequested)
        {
            try
            {
                var metrics = await GetMetricsAsync();
                MetricsUpdated?.Invoke(this, metrics);
                
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring RDP performance");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }
    }
}
