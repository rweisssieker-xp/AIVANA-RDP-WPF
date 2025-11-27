using System.Windows;
using WpfApplication = System.Windows.Application;
using WpfMessageBox = System.Windows.MessageBox;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public void ShowInfo(string message, string? title = null)
    {
        _logger.LogInformation("Notification: {Message}", message);
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            WpfMessageBox.Show(message, title ?? "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    public void ShowSuccess(string message, string? title = null)
    {
        _logger.LogInformation("Success notification: {Message}", message);
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            WpfMessageBox.Show(message, title ?? "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    public void ShowWarning(string message, string? title = null)
    {
        _logger.LogWarning("Warning notification: {Message}", message);
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            WpfMessageBox.Show(message, title ?? "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        });
    }

    public void ShowError(string message, string? title = null)
    {
        _logger.LogError("Error notification: {Message}", message);
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
            WpfMessageBox.Show(message, title ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }
}

