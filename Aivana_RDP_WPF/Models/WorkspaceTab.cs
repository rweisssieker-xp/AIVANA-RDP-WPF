namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a workspace tab in the unified interface
/// </summary>
public class WorkspaceTab 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public TabType Type { get; set; }
    public object? Content { get; set; }
    public bool IsActive { get; set; }
    public bool IsClosable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivatedAt { get; set; }
    public string Icon { get; set; } = string.Empty;
    public TabStatus Status { get; set; } = TabStatus.Loading;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of workspace tabs
/// </summary>
public enum TabType 
{
    RemoteSession,
    LocalTerminal,
    FileExplorer,
    Settings,
    Dashboard,
    Custom
}

/// <summary>
/// Status of a workspace tab
/// </summary>
public enum TabStatus 
{
    Loading,
    Connected,
    Disconnected,
    Error,
    Ready
}
