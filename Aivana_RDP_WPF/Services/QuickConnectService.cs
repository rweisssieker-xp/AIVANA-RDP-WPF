using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Infrastructure.Protocols;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of quick connect service
/// </summary>
public class QuickConnectService : IQuickConnectService 
{
    private readonly IConnectionProfileService _profileService;
    private readonly IProtocolFactory _protocolFactory;
    private readonly ILogger<QuickConnectService> _logger;
    private readonly List<string> _recentQueries = new();

    public QuickConnectService(
        IConnectionProfileService profileService,
        IProtocolFactory protocolFactory,
        ILogger<QuickConnectService> logger)
    {
        _profileService = profileService;
        _protocolFactory = protocolFactory;
        _logger = logger;
    }

    public async Task<List<ConnectionSuggestion>> GetSuggestionsAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetFavoriteSuggestionsAsync(ct);
        }

        var suggestions = new List<ConnectionSuggestion>();
        var allProfiles = await _profileService.GetAllProfilesAsync();

        var quickConnectProfile = ParseConnectionString(query);
        if (quickConnectProfile != null)
        {
            suggestions.Add(new ConnectionSuggestion
            {
                Profile = quickConnectProfile,
                Type = SuggestionType.HostMatch,
                MatchedText = query,
                RelevanceScore = 1.0
            });
        }

        foreach (var profile in allProfiles)
        {
            double score = 0;
            var type = SuggestionType.NameMatch;

            if (profile.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.8;
                type = SuggestionType.NameMatch;
            }

            if (profile.ServerAddress.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.9;
                type = SuggestionType.HostMatch;
            }

            var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(profile.Tags) ?? new List<string>();
            if (tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
            {
                score += 0.6;
                type = SuggestionType.TagMatch;
            }

            if (profile.IsFavorite) score += 0.3;

            if (profile.LastConnectedAt.HasValue)
            {
                var daysSinceLastConnect = (DateTime.UtcNow - profile.LastConnectedAt.Value).TotalDays;
                if (daysSinceLastConnect < 7) score += 0.2;
                if (daysSinceLastConnect < 1) score += 0.3;
            }

            if (profile.ConnectionCount > 10) score += 0.1;
            if (profile.ConnectionCount > 50) score += 0.2;

            if (score > 0)
            {
                suggestions.Add(new ConnectionSuggestion
                {
                    Profile = profile,
                    Type = type,
                    MatchedText = query,
                    LastConnectedAt = profile.LastConnectedAt ?? DateTime.MinValue,
                    ConnectionCount = profile.ConnectionCount,
                    RelevanceScore = score
                });
            }
        }

        return suggestions
            .OrderByDescending(s => s.RelevanceScore)
            .ThenByDescending(s => s.LastConnectedAt)
            .Take(10)
            .ToList();
    }

    public async Task<ConnectionProfile> CreateProfileFromQuickConnectAsync(string connectionString, CancellationToken ct = default)
    {
        var profile = ParseConnectionString(connectionString);
        if (profile == null)
        {
            throw new ArgumentException($"Invalid connection string: {connectionString}");
        }

        var existingProfiles = await _profileService.GetAllProfilesAsync();
        var existing = existingProfiles.FirstOrDefault(p => 
            p.ServerAddress == profile.ServerAddress && 
            p.Port == profile.Port &&
            p.Username == profile.Username);

        if (existing != null)
        {
            return existing;
        }

        profile.Name = $"{profile.ServerAddress}:{profile.Port}";
        profile.CreatedAt = DateTime.UtcNow;
        
        return await _profileService.CreateProfileAsync(profile);
    }

    public async Task<ConnectionResult> QuickConnectAsync(string connectionString, CancellationToken ct = default)
    {
        try
        {
            var profile = await CreateProfileFromQuickConnectAsync(connectionString, ct);
            
            var protocol = _protocolFactory.CreateProtocol(profile.ProtocolType);
            
            return await protocol.ConnectAsync(profile, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quick connect failed for {ConnectionString}", connectionString);
            return ConnectionResult.Failed($"Quick connect failed: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> GetRecentQueriesAsync(int count = 10)
    {
        await Task.CompletedTask;
        return _recentQueries.Take(count).ToList();
    }

    public async Task AddRecentQueryAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;

        _recentQueries.Remove(query);
        _recentQueries.Insert(0, query);
        
        if (_recentQueries.Count > 50)
        {
            _recentQueries.RemoveAt(_recentQueries.Count - 1);
        }

        await Task.CompletedTask;
    }

    private async Task<List<ConnectionSuggestion>> GetFavoriteSuggestionsAsync(CancellationToken ct)
    {
        var allProfiles = await _profileService.GetAllProfilesAsync();
        
        return allProfiles
            .Where(p => p.IsFavorite)
            .OrderByDescending(p => p.LastConnectedAt)
            .Take(5)
            .Select(p => new ConnectionSuggestion
            {
                Profile = p,
                Type = SuggestionType.Favorite,
                MatchedText = p.Name,
                LastConnectedAt = p.LastConnectedAt ?? DateTime.MinValue,
                ConnectionCount = p.ConnectionCount,
                RelevanceScore = 0.8
            })
            .ToList();
    }

    private ConnectionProfile? ParseConnectionString(string connectionString)
    {
        var patterns = new[]
        {
            @"^(?<protocol>rdp|ssh|vnc)://(?<username>[^@]+)@(?<host>[^:]+):(?<port>\d+)$",
            @"^(?<protocol>rdp|ssh|vnc)://(?<host>[^:]+):(?<port>\d+)$",
            @"^(?<username>[^@]+)@(?<host>[^:]+):(?<port>\d+)$",
            @"^(?<host>[^:]+):(?<port>\d+)$",
            @"^(?<host>[^:]+)$"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(connectionString, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var protocol = match.Groups["protocol"].Success ? 
                    match.Groups["protocol"].Value.ToLowerInvariant() switch
                    {
                        "rdp" => ProtocolType.RDP,
                        "ssh" => ProtocolType.SSH,
                        "vnc" => ProtocolType.VNC,
                        _ => ProtocolType.RDP
                    } : ProtocolType.RDP;

                var host = match.Groups["host"].Value;
                var port = match.Groups["port"].Success ? int.Parse(match.Groups["port"].Value) : 
                    protocol switch
                    {
                        ProtocolType.RDP => 3389,
                        ProtocolType.SSH => 22,
                        ProtocolType.VNC => 5900,
                        _ => 3389
                    };

                var username = match.Groups["username"].Success ? match.Groups["username"].Value : null;

                return new ConnectionProfile
                {
                    ProtocolType = protocol,
                    ServerAddress = host,
                    Port = port,
                    Username = username
                };
            }
        }

        return null;
    }
}
