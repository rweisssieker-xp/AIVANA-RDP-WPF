using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.Helpers;

namespace Aivana_RDP_WPF.ViewModels.ConnectionManagement;

/// <summary>
/// ViewModel for connection configuration dialog.
/// </summary>
public partial class ConnectionConfigViewModel : ObservableObject
{
    private readonly IConnectionProfileService _connectionProfileService;
    private readonly ILogger<ConnectionConfigViewModel> _logger;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _serverAddress = string.Empty;

    [ObservableProperty]
    private int _port = 3389;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _domain;

    [ObservableProperty]
    private string _resolution = "1920x1080";

    [ObservableProperty]
    private int _colorDepth = 32;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int? _editingProfileId;

    [ObservableProperty]
    private bool _savePassword;

    [ObservableProperty]
    private string? _password;

    private readonly ICredentialService? _credentialService;

    public ConnectionConfigViewModel(
        IConnectionProfileService connectionProfileService,
        ILogger<ConnectionConfigViewModel> logger,
        ICredentialService? credentialService = null)
    {
        _connectionProfileService = connectionProfileService;
        _credentialService = credentialService;
        _logger = logger;
    }

    public void LoadProfile(ConnectionProfile profile)
    {
        IsEditMode = true;
        EditingProfileId = profile.Id;
        Name = profile.Name;
        ServerAddress = profile.ServerAddress;
        Port = profile.Port;
        Username = profile.Username;
        Domain = profile.Domain;
        
        // Parse settings if needed
        if (!string.IsNullOrEmpty(profile.Settings))
        {
            // Settings parsing can be added later
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (IsEditMode && EditingProfileId.HasValue)
            {
                var profile = await _connectionProfileService.GetProfileByIdAsync(EditingProfileId.Value);
                if (profile != null)
                {
                    profile.Name = Name;
                    profile.ServerAddress = ServerAddress;
                    profile.Port = Port;
                    profile.Username = Username;
                    profile.Domain = Domain;
                    
                    await _connectionProfileService.UpdateProfileAsync(profile);
                    _logger.LogInformation("Updated connection profile {ProfileId}", profile.Id);
                    
                    // Update credentials if password provided and SavePassword is checked
                    if (SavePassword && !string.IsNullOrEmpty(Password) && _credentialService != null)
                    {
                        await _credentialService.SaveCredentialsAsync(profile.Id, Username ?? "", Password ?? "");
                    }
                    else if (!SavePassword && _credentialService != null)
                    {
                        // Delete credentials if SavePassword is unchecked
                        await _credentialService.DeleteCredentialsAsync(profile.Id);
                    }
                }
            }
            else
            {
                var profile = new ConnectionProfile
                {
                    Name = Name,
                    ServerAddress = ServerAddress,
                    Port = Port,
                    Username = Username,
                    Domain = Domain,
                    Settings = "{}"
                };
                
                var createdProfile = await _connectionProfileService.CreateProfileAsync(profile);
                _logger.LogInformation("Created connection profile {ProfileId}", createdProfile.Id);
                
                // Save credentials if password provided and SavePassword is checked
                if (SavePassword && !string.IsNullOrEmpty(Password) && _credentialService != null)
                {
                    await _credentialService.SaveCredentialsAsync(createdProfile.Id, Username ?? "", Password ?? "");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving connection profile");
            throw;
        }
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;
        if (!ValidationHelper.IsValidServerAddress(ServerAddress))
            return false;
        if (!ValidationHelper.IsValidPort(Port))
            return false;
        return true;
    }
}

