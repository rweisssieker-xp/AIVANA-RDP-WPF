namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a workspace layout configuration
/// </summary>
public class WorkspaceLayout 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<WorkspaceTab> Tabs { get; set; } = new();
    public string ActiveTabId { get; set; } = string.Empty;
    public LayoutConfiguration Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsDefault { get; set; }
}

/// <summary>
/// Layout configuration settings
/// </summary>
public class LayoutConfiguration 
{
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
    public bool IsMaximized { get; set; }
    public double SplitterPosition { get; set; } = 0.7;
    public TabBarPosition TabBarPosition { get; set; } = TabBarPosition.Top;
    public bool ShowTabIcons { get; set; } = true;
    public bool ShowTabCloseButtons { get; set; } = true;
}

/// <summary>
/// Position of the tab bar
/// </summary>
public enum TabBarPosition 
{
    Top,
    Bottom,
    Left,
    Right
}
