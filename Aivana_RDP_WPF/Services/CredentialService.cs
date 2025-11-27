using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Infrastructure.Credentials;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for credential management using Windows Credential Manager.
/// </summary>
public class CredentialService : ICredentialService
{
    private readonly ILogger<CredentialService> _logger;
    private readonly WindowsCredentialManager _credentialManager;

    public CredentialService(ILogger<CredentialService> logger)
    {
        _logger = logger;
        _credentialManager = new WindowsCredentialManager(logger);
    }

    public Task SaveCredentialsAsync(int connectionProfileId, string username, string password, CancellationToken ct = default)
    {
        _logger.LogInformation("Saving credentials for connection profile {ProfileId}", connectionProfileId);
        var targetName = GetCredentialTargetName(connectionProfileId);
        var success = _credentialManager.SaveCredential(targetName, username, password);
        if (!success)
        {
            _logger.LogWarning("Failed to save credentials for connection profile {ProfileId}", connectionProfileId);
        }
        return Task.CompletedTask;
    }

    public Task<Credential?> GetCredentialsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving credentials for connection profile {ProfileId}", connectionProfileId);
        var targetName = GetCredentialTargetName(connectionProfileId);
        var (username, password) = _credentialManager.ReadCredential(targetName);
        
        if (username != null && password != null)
        {
            return Task.FromResult<Credential?>(new Credential { Username = username, Password = password });
        }
        
        return Task.FromResult<Credential?>(null);
    }

    public Task DeleteCredentialsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting credentials for connection profile {ProfileId}", connectionProfileId);
        var targetName = GetCredentialTargetName(connectionProfileId);
        _credentialManager.DeleteCredential(targetName);
        return Task.CompletedTask;
    }

    private static string GetCredentialTargetName(int connectionProfileId)
    {
        return $"Aivana_RDP_WPF_Profile_{connectionProfileId}";
    }
}

