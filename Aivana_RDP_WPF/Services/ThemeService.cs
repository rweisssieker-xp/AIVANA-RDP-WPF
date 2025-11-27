using System.Windows;
using WpfApplication = System.Windows.Application;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for theme management.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private string _currentTheme = "Light";

    public string CurrentTheme => _currentTheme;

    public event EventHandler<string>? ThemeChanged;

    public ThemeService(ILogger<ThemeService> logger)
    {
        _logger = logger;
    }

    public void SetTheme(string themeName)
    {
        if (_currentTheme == themeName)
            return;

        try
        {
            _logger.LogInformation("Switching theme from {OldTheme} to {NewTheme}", _currentTheme, themeName);
            
            var app = WpfApplication.Current;
            if (app == null) return;

            // Remove existing theme resources
            var existingTheme = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.ToString().Contains("Themes/") == true);
            
            if (existingTheme != null)
            {
                app.Resources.MergedDictionaries.Remove(existingTheme);
            }

            // Add new theme
            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Resources/Themes/{themeName}Theme.xaml", UriKind.Absolute)
            };
            
            app.Resources.MergedDictionaries.Add(newTheme);
            
            _currentTheme = themeName;
            ThemeChanged?.Invoke(this, themeName);
            
            _logger.LogInformation("Theme switched successfully to {Theme}", themeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching theme to {Theme}", themeName);
        }
    }

    public IEnumerable<string> GetAvailableThemes()
    {
        return new[] { "Light", "Dark" };
    }
}

