using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of session manager service
/// </summary>
public class SessionManagerService : ISessionManagerService 
{
    private readonly ILogger<SessionManagerService> _logger;
    private readonly Dictionary<string, RemoteSession> _sessions = new();

    public event EventHandler<RemoteSession>? SessionCreated;
    public event EventHandler<RemoteSession>? SessionClosed;
    public event EventHandler<RemoteSession>? SessionStatusChanged;

    public SessionManagerService(ILogger<SessionManagerService> logger)
    {
        _logger = logger;
    }

    public async Task<RemoteSession> CreateSessionAsync(ConnectionProfile profile)
    {
        var session = new RemoteSession
        {
            Profile = profile,
            Status = SessionStatus.Connecting,
            CreatedAt = DateTime.UtcNow
        };

        _sessions[session.Id] = session;

        try
        {
            // TODO: Actually connect to the remote session
            await Task.Delay(1000); // Simulate connection time
            
            session.Status = SessionStatus.Connected;
            session.ConnectedAt = DateTime.UtcNow;
            
            SessionCreated?.Invoke(this, session);
            _logger.LogInformation("Created session {SessionId} for profile {ProfileName}", 
                session.Id, profile.Name);
        }
        catch (Exception ex)
        {
            session.Status = SessionStatus.Error;
            _logger.LogError(ex, "Failed to create session for profile {ProfileName}", profile.Name);
        }

        return session;
    }

    public async Task CloseSessionAsync(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("Session {SessionId} not found for closing", sessionId);
            return;
        }

        try
        {
            // TODO: Actually disconnect the session
            await Task.Delay(500); // Simulate disconnection time
            
            session.Status = SessionStatus.Disconnected;
            session.DisconnectedAt = DateTime.UtcNow;
            
            _sessions.Remove(sessionId);
            
            SessionClosed?.Invoke(this, session);
            _logger.LogInformation("Closed session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing session {SessionId}", sessionId);
        }
    }

    public async Task<List<RemoteSession>> GetActiveSessionsAsync()
    {
        return await Task.FromResult(_sessions.Values.ToList());
    }

    public async Task<RemoteSession?> GetSessionAsync(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return await Task.FromResult(session);
    }

    public async Task SuspendSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Status = SessionStatus.Suspended;
            SessionStatusChanged?.Invoke(this, session);
            _logger.LogInformation("Suspended session {SessionId}", sessionId);
        }
        
        await Task.CompletedTask;
    }

    public async Task ResumeSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Status = SessionStatus.Connected;
            SessionStatusChanged?.Invoke(this, session);
            _logger.LogInformation("Resumed session {SessionId}", sessionId);
        }
        
        await Task.CompletedTask;
    }
}
