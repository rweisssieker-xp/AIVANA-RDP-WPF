using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.Infrastructure.Protocols;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for quick connect functionality
/// </summary>
public partial class QuickConnectViewModel : ObservableObject
{
    private readonly IQuickConnectService _quickConnectService;
    private readonly IProtocolFactory _protocolFactory;

    [ObservableProperty]
    private string _connectionString = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ConnectionSuggestion> _suggestions = new();

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private bool _isSuggestionsVisible;

    [ObservableProperty]
    private ConnectionSuggestion? _selectedSuggestion;

    public QuickConnectViewModel(
        IQuickConnectService quickConnectService,
        IProtocolFactory protocolFactory)
    {
        _quickConnectService = quickConnectService;
        _protocolFactory = protocolFactory;
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            return;

        IsConnecting = true;
        try
        {
            var result = await _quickConnectService.QuickConnectAsync(ConnectionString);
            if (result.Success)
            {
                ConnectionString = string.Empty;
                Suggestions.Clear();
                IsSuggestionsVisible = false;
            }
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    private async Task SuggestionSelectedAsync(ConnectionSuggestion suggestion)
    {
        if (suggestion?.Profile == null) return;

        SelectedSuggestion = suggestion;
        ConnectionString = $"{suggestion.Profile.ServerAddress}:{suggestion.Profile.Port}";
        IsSuggestionsVisible = false;
        
        await ConnectAsync();
    }

    [RelayCommand]
    private async Task SearchSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            Suggestions.Clear();
            IsSuggestionsVisible = false;
            return;
        }

        try
        {
            var suggestions = await _quickConnectService.GetSuggestionsAsync(ConnectionString);
            Suggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                Suggestions.Add(suggestion);
            }
            
            IsSuggestionsVisible = Suggestions.Count > 0;
        }
        catch (Exception)
        {
            // Log error but don't crash
            Suggestions.Clear();
            IsSuggestionsVisible = false;
        }
    }

    partial void OnConnectionStringChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _ = Task.Delay(300).ContinueWith(async _ => await SearchSuggestionsAsync());
        }
        else
        {
            Suggestions.Clear();
            IsSuggestionsVisible = false;
        }
    }
}
