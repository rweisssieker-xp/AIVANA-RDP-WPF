namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Application-wide settings model.
/// </summary>
public class ApplicationSettings
{
    public DefaultConnectionSettings DefaultConnection { get; set; } = new();
    public UIPreferences UI { get; set; } = new();
    public UpdatePreferences Updates { get; set; } = new();
    public PerformanceSettings Performance { get; set; } = new();
}

/// <summary>
/// Default connection settings.
/// </summary>
public class DefaultConnectionSettings
{
    public int Port { get; set; } = 3389;
    public int ColorDepth { get; set; } = 32;
    public string Resolution { get; set; } = "1920x1080";
}

/// <summary>
/// UI preferences.
/// </summary>
public class UIPreferences
{
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "en-US";
}

/// <summary>
/// Update preferences.
/// </summary>
public class UpdatePreferences
{
    public bool AutoUpdate { get; set; } = false;
    public string CheckFrequency { get; set; } = "Weekly";
}

/// <summary>
/// Performance settings.
/// </summary>
public class PerformanceSettings
{
    public int MaxConcurrentConnections { get; set; } = 10;
    public int ConnectionTimeout { get; set; } = 30000;
}

