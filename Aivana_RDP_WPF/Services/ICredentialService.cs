namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for credential management.
/// </summary>
public class Credential
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public interface ICredentialService
{
    Task SaveCredentialsAsync(int connectionProfileId, string username, string password, CancellationToken ct = default);
    Task<Credential?> GetCredentialsAsync(int connectionProfileId, CancellationToken ct = default);
    Task DeleteCredentialsAsync(int connectionProfileId, CancellationToken ct = default);
}

