using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;

namespace Aivana_RDP_WPF.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly ISessionHistoryService _sessionHistoryService;
    private readonly ILogger<HistoryViewModel> _logger;

    public ObservableCollection<SessionHistory> Entries { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    public HistoryViewModel(ISessionHistoryService sessionHistoryService, ILogger<HistoryViewModel> logger)
    {
        _sessionHistoryService = sessionHistoryService;
        _logger = logger;

        _ = Task.Run(() => RefreshAsync());
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            Entries.Clear();

            var items = await _sessionHistoryService.GetRecentGlobalAsync(200, ct);
            foreach (var item in items)
            {
                Entries.Add(item);
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error loading global session history");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
