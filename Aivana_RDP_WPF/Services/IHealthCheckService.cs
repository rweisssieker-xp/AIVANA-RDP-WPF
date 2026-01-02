using System.Threading;
using System.Threading.Tasks;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

public interface IHealthCheckService
{
    Task<HealthCheckResult> CheckAsync(ConnectionProfile profile, CancellationToken ct = default);
}
