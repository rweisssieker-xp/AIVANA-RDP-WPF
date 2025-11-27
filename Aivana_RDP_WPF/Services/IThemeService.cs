namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for theme management.
/// </summary>
public interface IThemeService
{
    string CurrentTheme { get; }
    event EventHandler<string>? ThemeChanged;
    void SetTheme(string themeName);
    IEnumerable<string> GetAvailableThemes();
}

