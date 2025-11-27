using System.IO;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for importing and exporting connection profiles.
/// </summary>
public class ImportExportService : IImportExportService
{
    private readonly ILogger<ImportExportService> _logger;

    public ImportExportService(ILogger<ImportExportService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<ConnectionProfile>> ImportFromRdpFileAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Importing from RDP file: {FilePath}", filePath);
        var profiles = new List<ConnectionProfile>();
        
        try
        {
            var lines = File.ReadAllLines(filePath);
            var profile = new ConnectionProfile();
            
            foreach (var line in lines)
            {
                if (line.StartsWith("full address:s:", StringComparison.OrdinalIgnoreCase))
                {
                    var address = line.Substring("full address:s:".Length).Trim();
                    var parts = address.Split(':');
                    profile.ServerAddress = parts[0];
                    if (parts.Length > 1 && int.TryParse(parts[1], out var port))
                    {
                        profile.Port = port;
                    }
                }
                else if (line.StartsWith("username:s:", StringComparison.OrdinalIgnoreCase))
                {
                    profile.Username = line.Substring("username:s:".Length).Trim();
                }
                else if (line.StartsWith("domain:s:", StringComparison.OrdinalIgnoreCase))
                {
                    profile.Domain = line.Substring("domain:s:".Length).Trim();
                }
            }
            
            if (!string.IsNullOrEmpty(profile.ServerAddress))
            {
                profile.Name = profile.ServerAddress;
                profiles.Add(profile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from RDP file: {FilePath}", filePath);
        }
        
        return Task.FromResult<IEnumerable<ConnectionProfile>>(profiles);
    }

    public Task<IEnumerable<ConnectionProfile>> ImportFromJsonFileAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Importing from JSON file: {FilePath}", filePath);
        var profiles = new List<ConnectionProfile>();
        
        try
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            profiles = JsonSerializer.Deserialize<List<ConnectionProfile>>(json, options) ?? new List<ConnectionProfile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from JSON file: {FilePath}", filePath);
        }
        
        return Task.FromResult<IEnumerable<ConnectionProfile>>(profiles);
    }

    public Task<IEnumerable<ConnectionProfile>> ImportFromCsvFileAsync(string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Importing from CSV file: {FilePath}", filePath);
        var profiles = new List<ConnectionProfile>();
        
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return Task.FromResult<IEnumerable<ConnectionProfile>>(profiles);
            
            var headers = lines[0].Split(',');
            var nameIndex = Array.IndexOf(headers, "Name");
            var serverIndex = Array.IndexOf(headers, "ServerAddress");
            var portIndex = Array.IndexOf(headers, "Port");
            var usernameIndex = Array.IndexOf(headers, "Username");
            var domainIndex = Array.IndexOf(headers, "Domain");
            
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (serverIndex < 0 || values.Length <= serverIndex) continue;
                
                var profile = new ConnectionProfile
                {
                    Name = nameIndex >= 0 && nameIndex < values.Length ? values[nameIndex] : values[serverIndex],
                    ServerAddress = values[serverIndex],
                    Port = portIndex >= 0 && portIndex < values.Length && int.TryParse(values[portIndex], out var p) ? p : 3389,
                    Username = usernameIndex >= 0 && usernameIndex < values.Length ? values[usernameIndex] : null,
                    Domain = domainIndex >= 0 && domainIndex < values.Length ? values[domainIndex] : null
                };
                
                profiles.Add(profile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from CSV file: {FilePath}", filePath);
        }
        
        return Task.FromResult<IEnumerable<ConnectionProfile>>(profiles);
    }

    public Task ExportToRdpFileAsync(ConnectionProfile profile, string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting to RDP file: {FilePath}", filePath);
        
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("screen mode id:i:2");
            sb.AppendLine($"full address:s:{profile.ServerAddress}:{profile.Port}");
            if (!string.IsNullOrEmpty(profile.Username))
            {
                sb.AppendLine($"username:s:{profile.Username}");
            }
            if (!string.IsNullOrEmpty(profile.Domain))
            {
                sb.AppendLine($"domain:s:{profile.Domain}");
            }
            
            File.WriteAllText(filePath, sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to RDP file: {FilePath}", filePath);
            throw;
        }
        
        return Task.CompletedTask;
    }

    public Task ExportToJsonFileAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting to JSON file: {FilePath}", filePath);
        
        try
        {
            var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to JSON file: {FilePath}", filePath);
            throw;
        }
        
        return Task.CompletedTask;
    }

    public Task ExportToCsvFileAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting to CSV file: {FilePath}", filePath);
        
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Name,ServerAddress,Port,Username,Domain");
            
            foreach (var profile in profiles)
            {
                sb.AppendLine($"{profile.Name},{profile.ServerAddress},{profile.Port},{profile.Username ?? ""},{profile.Domain ?? ""}");
            }
            
            File.WriteAllText(filePath, sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to CSV file: {FilePath}", filePath);
            throw;
        }
        
        return Task.CompletedTask;
    }
}

