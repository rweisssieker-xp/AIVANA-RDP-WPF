namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for notifications.
/// </summary>
public interface INotificationService
{
    void ShowInfo(string message, string? title = null);
    void ShowSuccess(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    void ShowError(string message, string? title = null);
}
