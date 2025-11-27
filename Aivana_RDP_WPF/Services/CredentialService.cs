using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for credential management using Windows Credential Manager.
/// </summary>
public class CredentialService : ICredentialService
{
    private readonly ILogger<CredentialService> _logger;

    public CredentialService(ILogger<CredentialService> logger)
    {
        _logger = logger;
    }

    public Task SaveCredentialsAsync(int connectionProfileId, string username, string password, CancellationToken ct = default)
    {
        _logger.LogInformation("Saving credentials for connection profile {ProfileId}", connectionProfileId);
        // Windows Credential Manager implementation will be added in Story 2.7
        return Task.CompletedTask;
    }

    public Task<Credential?> GetCredentialsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving credentials for connection profile {ProfileId}", connectionProfileId);
        // Windows Credential Manager implementation will be added in Story 2.7
        return Task.FromResult<Credential?>(null);
    }

    public Task DeleteCredentialsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting credentials for connection profile {ProfileId}", connectionProfileId);
        // Windows Credential Manager implementation will be added in Story 2.7
        return Task.CompletedTask;
    }
}

