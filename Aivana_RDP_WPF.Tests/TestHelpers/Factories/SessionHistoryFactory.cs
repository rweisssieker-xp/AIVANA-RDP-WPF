using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Factories;

/// <summary>
/// Factory for creating test SessionHistory instances
/// </summary>
public static class SessionHistoryFactory
{
    public static SessionHistory Create(
        Guid connectionProfileId,
        DateTime? connectedAt = null,
        DateTime? disconnectedAt = null,
        string status = "Connected",
        string? errorMessage = null)
    {
        var connected = connectedAt ?? DateTime.UtcNow.AddMinutes(-10);
        var disconnected = disconnectedAt ?? DateTime.UtcNow;

        return new SessionHistory
        {
            Id = Guid.NewGuid(),
            ConnectionProfileId = connectionProfileId,
            ConnectedAt = connected,
            DisconnectedAt = disconnected,
            Duration = disconnected - connected,
            Status = status,
            ErrorMessage = errorMessage
        };
    }

    public static List<SessionHistory> CreateRecentSessions(
        Guid connectionProfileId,
        int count = 10)
    {
        var sessions = new List<SessionHistory>();
        for (int i = 0; i < count; i++)
        {
            sessions.Add(Create(
                connectionProfileId,
                connectedAt: DateTime.UtcNow.AddHours(-i),
                disconnectedAt: DateTime.UtcNow.AddHours(-i).AddMinutes(30)
            ));
        }
        return sessions;
    }
}

