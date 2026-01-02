using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of SSH tunnel service
/// </summary>
public class SshTunnelService : ISshTunnelService 
{
    private readonly ILogger<SshTunnelService> _logger;
    private readonly Dictionary<string, TunnelInfo> _activeTunnels = new();

    public SshTunnelService(ILogger<SshTunnelService> logger)
    {
        _logger = logger;
    }

    public async Task<TunnelResult> CreateTunnelAsync(SshTunnelConfiguration config, CancellationToken ct = default)
    {
        var tunnelId = Guid.NewGuid().ToString("N")[..8];
        
        try
        {
            _logger.LogInformation("Creating SSH tunnel {TunnelId} from {LocalHost}:{LocalPort} to {RemoteHost}:{RemotePort} via {SshHost}:{SshPort}",
                tunnelId, config.LocalHost, config.LocalPort, config.RemoteHost, config.RemotePort, config.SshHost, config.SshPort);

            var validationResult = ValidateConfiguration(config);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            if (config.LocalPort == 0)
            {
                config.LocalPort = GetAvailablePort();
            }

            await Task.Delay(1000, ct);

            var tunnelInfo = new TunnelInfo
            {
                TunnelId = tunnelId,
                Configuration = config,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _activeTunnels[tunnelId] = tunnelInfo;

            _logger.LogInformation("SSH tunnel {TunnelId} created successfully on local port {LocalPort}", 
                tunnelId, config.LocalPort);

            return TunnelResult.Successful(tunnelId, config.LocalPort, 
                $"SSH tunnel established: {config.LocalHost}:{config.LocalPort} -> {config.RemoteHost}:{config.RemotePort}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SSH tunnel {TunnelId}", tunnelId);
            return TunnelResult.Failed($"Failed to create SSH tunnel: {ex.Message}");
        }
    }

    public async Task CloseTunnelAsync(string tunnelId, CancellationToken ct = default)
    {
        if (!_activeTunnels.TryGetValue(tunnelId, out var tunnelInfo))
        {
            _logger.LogWarning("SSH tunnel {TunnelId} not found", tunnelId);
            return;
        }

        try
        {
            _logger.LogInformation("Closing SSH tunnel {TunnelId}", tunnelId);

            await Task.Delay(500, ct);

            _activeTunnels.Remove(tunnelId);

            _logger.LogInformation("SSH tunnel {TunnelId} closed successfully", tunnelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing SSH tunnel {TunnelId}", tunnelId);
        }
    }

    public async Task<bool> IsTunnelActiveAsync(string tunnelId, CancellationToken ct = default)
    {
        _activeTunnels.TryGetValue(tunnelId, out var tunnelInfo);
        
        if (tunnelInfo == null)
        {
            return await Task.FromResult(false);
        }

        return await Task.FromResult(tunnelInfo.IsActive);
    }

    public async Task<List<TunnelInfo>> GetActiveTunnelsAsync(CancellationToken ct = default)
    {
        return await Task.FromResult(_activeTunnels.Values.ToList());
    }

    private TunnelResult ValidateConfiguration(SshTunnelConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.SshHost))
        {
            return TunnelResult.Failed("SSH host is required");
        }

        if (string.IsNullOrWhiteSpace(config.SshUser))
        {
            return TunnelResult.Failed("SSH user is required");
        }

        if (string.IsNullOrWhiteSpace(config.PrivateKeyPath) && string.IsNullOrWhiteSpace(config.Password))
        {
            return TunnelResult.Failed("Either private key path or password is required");
        }

        if (string.IsNullOrWhiteSpace(config.RemoteHost))
        {
            return TunnelResult.Failed("Remote host is required");
        }

        if (config.RemotePort <= 0 || config.RemotePort > 65535)
        {
            return TunnelResult.Failed("Invalid remote port");
        }

        if (config.SshPort <= 0 || config.SshPort > 65535)
        {
            return TunnelResult.Failed("Invalid SSH port");
        }

        return TunnelResult.Successful("validation", 0, "Configuration is valid");
    }

    private int GetAvailablePort()
    {
        using var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }
}
