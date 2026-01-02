using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Aivana_RDP_WPF.Helpers;
using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(ILogger<HealthCheckService> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckAsync(ConnectionProfile profile, CancellationToken ct = default)
    {
        var result = new HealthCheckResult
        {
            Port = profile.Port
        };

        try
        {
            result.ResolvedIp = await NetworkHelper.ResolveHostnameAsync(profile.ServerAddress);

            using (var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(profile.ServerAddress, 3000);
                result.PingSuccess = reply.Status == IPStatus.Success;
                if (result.PingSuccess)
                {
                    result.PingRoundtripMs = reply.RoundtripTime;
                }
            }

            var sw = Stopwatch.StartNew();
            using (var tcp = new TcpClient())
            {
                using var reg = ct.Register(() =>
                {
                    try { tcp.Close(); } catch { }
                });

                var connectTask = tcp.ConnectAsync(profile.ServerAddress, profile.Port);
                await connectTask;
            }
            sw.Stop();

            result.PortOpen = true;
            result.TcpConnectMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for {Server}:{Port}", profile.ServerAddress, profile.Port);
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}
