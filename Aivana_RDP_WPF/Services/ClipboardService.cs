using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using System.Collections.Concurrent;
using WpfClipboard = System.Windows.Clipboard;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for clipboard synchronization.
/// </summary>
public class ClipboardService : IClipboardService
{
    private readonly ILogger<ClipboardService> _logger;
    private readonly ConcurrentDictionary<int, bool> _clipboardEnabled = new();
    private readonly ConcurrentDictionary<int, List<ClipboardItem>> _clipboardHistory = new();
    private int _nextClipboardItemId = 1;

    public event EventHandler<ClipboardItem>? ClipboardChanged;

    public ClipboardService(ILogger<ClipboardService> logger)
    {
        _logger = logger;
    }

    public Task SyncClipboardToRemoteAsync(int connectionProfileId, CancellationToken ct = default)
    {
        if (!IsClipboardEnabled(connectionProfileId))
        {
            _logger.LogDebug("Clipboard sync disabled for connection {ConnectionId}", connectionProfileId);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Syncing clipboard to remote for connection {ConnectionId}", connectionProfileId);
        
        try
        {
            var item = new ClipboardItem
            {
                Id = _nextClipboardItemId++,
                ConnectionProfileId = connectionProfileId,
                Format = ClipboardFormat.Text,
                CreatedAt = DateTime.UtcNow
            };

            // Get clipboard content (simplified - in real implementation, handle all formats)
            if (WpfClipboard.ContainsText())
            {
                var text = WpfClipboard.GetText();
                item.Data = System.Text.Encoding.UTF8.GetBytes(text);
                item.Preview = text.Length > 50 ? text.Substring(0, 50) + "..." : text;
            }
            else if (WpfClipboard.ContainsImage())
            {
                item.Format = ClipboardFormat.Image;
                var image = WpfClipboard.GetImage();
                // Convert image to bytes (simplified)
                item.Preview = "[Image]";
            }

            AddToHistory(connectionProfileId, item);
            ClipboardChanged?.Invoke(this, item);
            
            // In real implementation, send to RDP session via clipboard redirection
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing clipboard to remote for connection {ConnectionId}", connectionProfileId);
        }

        return Task.CompletedTask;
    }

    public Task SyncClipboardFromRemoteAsync(int connectionProfileId, CancellationToken ct = default)
    {
        if (!IsClipboardEnabled(connectionProfileId))
        {
            _logger.LogDebug("Clipboard sync disabled for connection {ConnectionId}", connectionProfileId);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Syncing clipboard from remote for connection {ConnectionId}", connectionProfileId);
        
        // In real implementation, receive from RDP session via clipboard redirection
        // and set local clipboard
        
        return Task.CompletedTask;
    }

    public Task<List<ClipboardItem>> GetClipboardHistoryAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving clipboard history for connection {ConnectionId}", connectionProfileId);
        if (_clipboardHistory.TryGetValue(connectionProfileId, out var history))
        {
            return Task.FromResult(history.OrderByDescending(h => h.CreatedAt).ToList());
        }
        return Task.FromResult(new List<ClipboardItem>());
    }

    public Task SetClipboardEnabledAsync(int connectionProfileId, bool enabled, CancellationToken ct = default)
    {
        _logger.LogInformation("Setting clipboard enabled={Enabled} for connection {ConnectionId}", enabled, connectionProfileId);
        _clipboardEnabled[connectionProfileId] = enabled;
        return Task.CompletedTask;
    }

    public bool IsClipboardEnabled(int connectionProfileId)
    {
        return _clipboardEnabled.GetValueOrDefault(connectionProfileId, true); // Default enabled
    }

    private void AddToHistory(int connectionProfileId, ClipboardItem item)
    {
        var history = _clipboardHistory.GetOrAdd(connectionProfileId, _ => new List<ClipboardItem>());
        history.Add(item);
        
        // Keep only last 50 items
        if (history.Count > 50)
        {
            history.RemoveAt(0);
        }
    }
}

