using Aivana_RDP_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of layout persistence service
/// </summary>
public class LayoutPersistenceService : ILayoutPersistenceService 
{
    private readonly ILogger<LayoutPersistenceService> _logger;
    private readonly Dictionary<string, WorkspaceLayout> _layouts = new();
    private string? _defaultLayoutId;

    public LayoutPersistenceService(ILogger<LayoutPersistenceService> logger)
    {
        _logger = logger;
        LoadDefaultLayouts();
    }

    public async Task SaveLayoutAsync(WorkspaceLayout layout)
    {
        try
        {
            layout.ModifiedAt = DateTime.UtcNow;
            _layouts[layout.Id] = layout;
            
            // TODO: Save to persistent storage (file, database, etc.)
            var json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
            
            _logger.LogInformation("Saved layout {LayoutName} with ID {LayoutId}", layout.Name, layout.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save layout {LayoutName}", layout.Name);
            throw;
        }
        
        await Task.CompletedTask;
    }

    public async Task<WorkspaceLayout?> LoadLayoutAsync(string layoutId)
    {
        try
        {
            if (_layouts.TryGetValue(layoutId, out var layout))
            {
                _logger.LogInformation("Loaded layout {LayoutName} from memory", layout.Name);
                return layout;
            }

            // TODO: Load from persistent storage
            _logger.LogWarning("Layout {LayoutId} not found", layoutId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load layout {LayoutId}", layoutId);
            return null;
        }
    }

    public async Task<List<WorkspaceLayout>> GetAllLayoutsAsync()
    {
        return await Task.FromResult(_layouts.Values.ToList());
    }

    public async Task DeleteLayoutAsync(string layoutId)
    {
        try
        {
            if (_layouts.Remove(layoutId))
            {
                if (_defaultLayoutId == layoutId)
                {
                    _defaultLayoutId = null;
                }
                
                // TODO: Delete from persistent storage
                _logger.LogInformation("Deleted layout {LayoutId}", layoutId);
            }
            else
            {
                _logger.LogWarning("Layout {LayoutId} not found for deletion", layoutId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete layout {LayoutId}", layoutId);
        }
        
        await Task.CompletedTask;
    }

    public async Task SetDefaultLayoutAsync(string layoutId)
    {
        if (_layouts.ContainsKey(layoutId))
        {
            _defaultLayoutId = layoutId;
            
            // Clear default flag from all layouts
            foreach (var layout in _layouts.Values)
            {
                layout.IsDefault = false;
            }
            
            // Set default flag on the specified layout
            _layouts[layoutId].IsDefault = true;
            
            _logger.LogInformation("Set layout {LayoutId} as default", layoutId);
        }
        else
        {
            _logger.LogWarning("Layout {LayoutId} not found for setting as default", layoutId);
        }
        
        await Task.CompletedTask;
    }

    public async Task<WorkspaceLayout?> GetDefaultLayoutAsync()
    {
        if (!string.IsNullOrEmpty(_defaultLayoutId) && _layouts.TryGetValue(_defaultLayoutId, out var defaultLayout))
        {
            return await Task.FromResult(defaultLayout);
        }

        // Fallback to first layout marked as default
        var fallbackLayout = _layouts.Values.FirstOrDefault(l => l.IsDefault);
        return await Task.FromResult(fallbackLayout);
    }

    private void LoadDefaultLayouts()
    {
        // Create a default layout
        var defaultLayout = new WorkspaceLayout
        {
            Name = "Default Workspace",
            IsDefault = true,
            Configuration = new LayoutConfiguration
            {
                WindowWidth = 1200,
                WindowHeight = 800,
                TabBarPosition = TabBarPosition.Top,
                ShowTabIcons = true,
                ShowTabCloseButtons = true
            }
        };

        _layouts[defaultLayout.Id] = defaultLayout;
        _defaultLayoutId = defaultLayout.Id;
        
        _logger.LogInformation("Loaded default layout {LayoutName}", defaultLayout.Name);
    }
}
