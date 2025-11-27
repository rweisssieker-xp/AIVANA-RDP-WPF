using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for application settings.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _selectedTheme;

    public SettingsViewModel(IThemeService themeService, ILogger<SettingsViewModel> logger)
    {
        _themeService = themeService;
        _logger = logger;
        _selectedTheme = _themeService.CurrentTheme;
        
        _themeService.ThemeChanged += (s, theme) => SelectedTheme = theme;
    }

    [RelayCommand]
    private void ChangeTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
            return;

        _logger.LogInformation("Changing theme to {Theme}", themeName);
        _themeService.SetTheme(themeName);
        SelectedTheme = themeName;
    }
}

