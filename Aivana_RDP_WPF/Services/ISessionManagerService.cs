using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for session management
/// </summary>
public interface ISessionManagerService 
{
    Task<RemoteSession> CreateSessionAsync(ConnectionProfile profile);
    Task CloseSessionAsync(string sessionId);
    Task<List<RemoteSession>> GetActiveSessionsAsync();
    Task<RemoteSession?> GetSessionAsync(string sessionId);
    Task SuspendSessionAsync(string sessionId);
    Task ResumeSessionAsync(string sessionId);
    event EventHandler<RemoteSession>? SessionCreated;
    event EventHandler<RemoteSession>? SessionClosed;
    event EventHandler<RemoteSession>? SessionStatusChanged;
}
