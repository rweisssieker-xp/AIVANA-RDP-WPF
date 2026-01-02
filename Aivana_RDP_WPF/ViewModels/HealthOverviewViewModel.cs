using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.ViewModels;

public partial class HealthOverviewViewModel : ObservableObject
{
    private readonly IConnectionProfileService _connectionProfileService;
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthOverviewViewModel> _logger;

    public ObservableCollection<ConnectionProfile> Profiles { get; } = new();

    [ObservableProperty]
    private ConnectionProfile? _selectedProfile;

    [ObservableProperty]
    private HealthCheckResult? _lastResult;

    [ObservableProperty]
    private bool _isChecking;

    public HealthOverviewViewModel(
        IConnectionProfileService connectionProfileService,
        IHealthCheckService healthCheckService,
        ILogger<HealthOverviewViewModel> logger)
    {
        _connectionProfileService = connectionProfileService;
        _healthCheckService = healthCheckService;
        _logger = logger;

        _ = Task.Run(() => LoadProfilesAsync());
    }

    [RelayCommand]
    private async Task LoadProfilesAsync(CancellationToken ct = default)
    {
        try
        {
            Profiles.Clear();
            var profiles = await _connectionProfileService.GetAllProfilesAsync(ct);
            foreach (var p in profiles)
            {
                Profiles.Add(p);
            }

            if (SelectedProfile == null && Profiles.Count > 0)
            {
                SelectedProfile = Profiles[0];
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error loading profiles for health overview");
        }
    }

    [RelayCommand]
    private async Task RunHealthCheckAsync(CancellationToken ct = default)
    {
        if (SelectedProfile == null)
        {
            return;
        }

        try
        {
            IsChecking = true;
            LastResult = await _healthCheckService.CheckAsync(SelectedProfile, ct);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error running health check");
        }
        finally
        {
            IsChecking = false;
        }
    }
}
