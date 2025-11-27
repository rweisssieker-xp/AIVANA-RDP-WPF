using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for managing multiple RDP sessions in a tabbed interface.
/// </summary>
public partial class MultiSessionViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MultiSessionViewModel> _logger;

    [ObservableProperty]
    private List<ConnectionSessionViewModel> _sessions = new();

    [ObservableProperty]
    private ConnectionSessionViewModel? _selectedSession;

    public MultiSessionViewModel(
        IServiceProvider serviceProvider,
        ILogger<MultiSessionViewModel> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [RelayCommand]
    private void AddSession(ConnectionProfile? profile)
    {
        if (profile == null) return;

        _logger.LogInformation("Adding new session for profile {ProfileId}", profile.Id);
        
        var session = _serviceProvider.GetRequiredService<ConnectionSessionViewModel>();
        session.LoadProfile(profile);
        Sessions.Add(session);
        SelectedSession = session;
    }

    [RelayCommand]
    private void CloseSession(ConnectionSessionViewModel? session)
    {
        if (session == null) return;

        _logger.LogInformation("Closing session");
        
        // Disconnect if connected
        if (session.IsConnected)
        {
            session.DisconnectCommand.Execute(null);
        }
        
        Sessions.Remove(session);
        
        if (SelectedSession == session)
        {
            SelectedSession = Sessions.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void SwitchSession(ConnectionSessionViewModel? session)
    {
        if (session != null)
        {
            // Update selection state
            foreach (var s in Sessions)
            {
                s.IsSelected = false;
            }
            session.IsSelected = true;
            SelectedSession = session;
        }
    }

    partial void OnSelectedSessionChanged(ConnectionSessionViewModel? value)
    {
        // Update selection state when SelectedSession changes
        foreach (var s in Sessions)
        {
            s.IsSelected = s == value;
        }
    }
}

