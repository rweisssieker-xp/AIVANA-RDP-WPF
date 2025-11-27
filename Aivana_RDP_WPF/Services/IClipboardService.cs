using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for clipboard synchronization.
/// </summary>
public interface IClipboardService
{
    Task SyncClipboardToRemoteAsync(int connectionProfileId, CancellationToken ct = default);
    Task SyncClipboardFromRemoteAsync(int connectionProfileId, CancellationToken ct = default);
    Task<List<ClipboardItem>> GetClipboardHistoryAsync(int connectionProfileId, CancellationToken ct = default);
    Task SetClipboardEnabledAsync(int connectionProfileId, bool enabled, CancellationToken ct = default);
    bool IsClipboardEnabled(int connectionProfileId);
    event EventHandler<ClipboardItem>? ClipboardChanged;
}

