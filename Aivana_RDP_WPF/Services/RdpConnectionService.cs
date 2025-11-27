using System.Windows.Forms.Integration;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Infrastructure.Rdp;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for RDP connection management.
/// </summary>
public class RdpConnectionService : IRdpConnectionService
{
    private readonly ILogger<RdpConnectionService> _logger;
    private readonly Dictionary<int, RdpClientWrapper> _activeConnections = new();

    public RdpConnectionService(ILogger<RdpConnectionService> logger)
    {
        _logger = logger;
    }

    public System.Windows.Forms.Integration.WindowsFormsHost? CreateConnectionHost(ConnectionProfile profile)
    {
        try
        {
            var wrapperLogger = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<Infrastructure.Rdp.RdpClientWrapper>();
            var wrapper = new Infrastructure.Rdp.RdpClientWrapper(wrapperLogger);
            var host = wrapper.CreateHost();
            _activeConnections[profile.Id] = wrapper;
            _logger.LogInformation("Created RDP connection host for profile {ProfileId}", profile.Id);
            return host;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating RDP connection host for profile {ProfileId}", profile.Id);
            throw;
        }
    }

    public async Task ConnectAsync(ConnectionProfile profile, string? password = null)
    {
        if (_activeConnections.TryGetValue(profile.Id, out var wrapper))
        {
            await Task.Run(() =>
            {
                wrapper.Connect(
                    profile.ServerAddress, 
                    profile.Port, 
                    profile.Username ?? "", 
                    profile.Domain,
                    password);
            });
            _logger.LogInformation("Connected to profile {ProfileId}", profile.Id);
        }
        else
        {
            throw new InvalidOperationException($"No connection host found for profile {profile.Id}");
        }
    }

    public void Connect(ConnectionProfile profile, string? password = null)
    {
        if (_activeConnections.TryGetValue(profile.Id, out var wrapper))
        {
            wrapper.Connect(profile.ServerAddress, profile.Port, profile.Username ?? "", profile.Domain, password);
            _logger.LogInformation("Connected to profile {ProfileId}", profile.Id);
        }
        else
        {
            throw new InvalidOperationException($"No connection host found for profile {profile.Id}");
        }
    }

    public void Disconnect(int profileId)
    {
        if (_activeConnections.TryGetValue(profileId, out var wrapper))
        {
            wrapper.Disconnect();
            _activeConnections.Remove(profileId);
            wrapper.Dispose();
            _logger.LogInformation("Disconnected from profile {ProfileId}", profileId);
        }
    }

    public bool IsConnected(int profileId)
    {
        if (_activeConnections.TryGetValue(profileId, out var wrapper))
        {
            return wrapper.IsConnected;
        }
        return false;
    }
}

