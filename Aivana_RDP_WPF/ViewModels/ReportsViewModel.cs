using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly ILogger<ReportsViewModel> _logger;

    public ObservableCollection<ReportFileInfo> Reports { get; } = new();

    [ObservableProperty]
    private ReportFileInfo? _selectedReport;

    [ObservableProperty]
    private bool _isLoading;

    public ReportsViewModel(ILogger<ReportsViewModel> logger)
    {
        _logger = logger;
        _ = Task.Run(() => RefreshAsync());
    }

    private static string GetReportsDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var reportsDirectory = Path.Combine(appDataPath, "Aivana_RDP_WPF", "Reports");
        if (!Directory.Exists(reportsDirectory))
        {
            Directory.CreateDirectory(reportsDirectory);
        }
        return reportsDirectory;
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            Reports.Clear();

            var dir = GetReportsDirectory();
            var files = Directory.EnumerateFiles(dir)
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Take(250)
                .ToList();

            foreach (var f in files)
            {
                Reports.Add(new ReportFileInfo
                {
                    FileName = f.Name,
                    FullPath = f.FullName,
                    Extension = f.Extension.TrimStart('.').ToLowerInvariant(),
                    SizeBytes = f.Length,
                    LastModifiedUtc = f.LastWriteTimeUtc
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports list");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenReportsFolder()
    {
        try
        {
            var dir = GetReportsDirectory();
            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening reports folder");
        }
    }

    [RelayCommand]
    private void OpenSelectedReport()
    {
        if (SelectedReport == null)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedReport.FullPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening report {Path}", SelectedReport.FullPath);
        }
    }
}

public class ReportFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
