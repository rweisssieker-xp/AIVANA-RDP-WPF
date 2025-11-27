using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Factories;

/// <summary>
/// Factory for creating test ConnectionProfile instances
/// </summary>
public static class ConnectionProfileFactory
{
    public static ConnectionProfile Create(
        string name,
        string serverAddress,
        int port = 3389,
        string? username = null,
        string? domain = null,
        bool isFavorite = false,
        string? groupName = null,
        List<string>? tags = null)
    {
        return new ConnectionProfile
        {
            Id = Guid.NewGuid(),
            Name = name,
            ServerAddress = serverAddress,
            Port = port,
            Username = username ?? "testuser",
            Domain = domain,
            IsFavorite = isFavorite,
            GroupName = groupName,
            Tags = tags ?? new List<string>(),
            Settings = "{}",
            CreatedAt = DateTime.UtcNow,
            LastConnectedAt = null,
            ConnectionCount = 0
        };
    }

    public static ConnectionProfile CreateWithSettings(
        string name,
        string serverAddress,
        Dictionary<string, object> settings)
    {
        var profile = Create(name, serverAddress);
        profile.Settings = System.Text.Json.JsonSerializer.Serialize(settings);
        return profile;
    }

    public static List<ConnectionProfile> CreateMultiple(int count, string prefix = "Test Server")
    {
        var profiles = new List<ConnectionProfile>();
        for (int i = 1; i <= count; i++)
        {
            profiles.Add(Create($"{prefix} {i}", $"192.168.1.{100 + i}"));
        }
        return profiles;
    }
}

