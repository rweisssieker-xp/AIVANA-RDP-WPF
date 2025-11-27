using System.IO;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using System.Collections.Concurrent;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for file transfer operations.
/// </summary>
public class FileTransferService : IFileTransferService
{
    private readonly ILogger<FileTransferService> _logger;
    private readonly ConcurrentDictionary<int, FileTransferInfo> _activeTransfers = new();
    private int _nextTransferId = 1;

    public event EventHandler<FileTransferInfo>? TransferProgress;
    public event EventHandler<FileTransferInfo>? TransferCompleted;

    public FileTransferService(ILogger<FileTransferService> logger)
    {
        _logger = logger;
    }

    public Task<FileTransferInfo> UploadFileAsync(int connectionProfileId, string localPath, string remotePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting file upload: {LocalPath} -> {RemotePath} for connection {ConnectionId}", localPath, remotePath, connectionProfileId);
        
        var transfer = new FileTransferInfo
        {
            Id = _nextTransferId++,
            ConnectionProfileId = connectionProfileId,
            LocalPath = localPath,
            RemotePath = remotePath,
            Direction = FileTransferDirection.Upload,
            Status = FileTransferStatus.Pending,
            StartedAt = DateTime.UtcNow
        };

        if (File.Exists(localPath))
        {
            transfer.TotalBytes = new FileInfo(localPath).Length;
        }

        _activeTransfers[transfer.Id] = transfer;
        
        // Simulate transfer (in real implementation, use RDP Virtual Channels)
        _ = Task.Run(async () => await SimulateTransferAsync(transfer, ct), ct);
        
        return Task.FromResult(transfer);
    }

    public Task<FileTransferInfo> DownloadFileAsync(int connectionProfileId, string remotePath, string localPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting file download: {RemotePath} -> {LocalPath} for connection {ConnectionId}", remotePath, localPath, connectionProfileId);
        
        var transfer = new FileTransferInfo
        {
            Id = _nextTransferId++,
            ConnectionProfileId = connectionProfileId,
            LocalPath = localPath,
            RemotePath = remotePath,
            Direction = FileTransferDirection.Download,
            Status = FileTransferStatus.Pending,
            StartedAt = DateTime.UtcNow
        };

        _activeTransfers[transfer.Id] = transfer;
        
        // Simulate transfer (in real implementation, use RDP Virtual Channels)
        _ = Task.Run(async () => await SimulateTransferAsync(transfer, ct), ct);
        
        return Task.FromResult(transfer);
    }

    public Task<List<FileTransferInfo>> GetTransferHistoryAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving transfer history for connection {ConnectionId}", connectionProfileId);
        var history = _activeTransfers.Values
            .Where(t => t.ConnectionProfileId == connectionProfileId)
            .OrderByDescending(t => t.StartedAt)
            .ToList();
        return Task.FromResult(history);
    }

    public Task PauseTransferAsync(int transferId, CancellationToken ct = default)
    {
        _logger.LogInformation("Pausing transfer {TransferId}", transferId);
        if (_activeTransfers.TryGetValue(transferId, out var transfer))
        {
            transfer.Status = FileTransferStatus.Paused;
        }
        return Task.CompletedTask;
    }

    public Task ResumeTransferAsync(int transferId, CancellationToken ct = default)
    {
        _logger.LogInformation("Resuming transfer {TransferId}", transferId);
        if (_activeTransfers.TryGetValue(transferId, out var transfer))
        {
            transfer.Status = FileTransferStatus.InProgress;
        }
        return Task.CompletedTask;
    }

    public Task CancelTransferAsync(int transferId, CancellationToken ct = default)
    {
        _logger.LogInformation("Cancelling transfer {TransferId}", transferId);
        if (_activeTransfers.TryGetValue(transferId, out var transfer))
        {
            transfer.Status = FileTransferStatus.Cancelled;
            _activeTransfers.TryRemove(transferId, out _);
        }
        return Task.CompletedTask;
    }

    private async Task SimulateTransferAsync(FileTransferInfo transfer, CancellationToken ct)
    {
        transfer.Status = FileTransferStatus.InProgress;
        
        try
        {
            var chunkSize = 1024 * 1024; // 1MB chunks
            var totalChunks = (int)Math.Ceiling((double)transfer.TotalBytes / chunkSize);
            
            for (int i = 0; i < totalChunks && !ct.IsCancellationRequested; i++)
            {
                if (transfer.Status == FileTransferStatus.Paused)
                {
                    await Task.Delay(100, ct);
                    i--; // Retry same chunk
                    continue;
                }
                
                if (transfer.Status == FileTransferStatus.Cancelled)
                {
                    break;
                }
                
                transfer.TransferredBytes = Math.Min(transfer.TransferredBytes + chunkSize, transfer.TotalBytes);
                TransferProgress?.Invoke(this, transfer);
                
                await Task.Delay(100, ct); // Simulate network delay
            }
            
            if (transfer.Status != FileTransferStatus.Cancelled)
            {
                transfer.Status = FileTransferStatus.Completed;
                transfer.CompletedAt = DateTime.UtcNow;
                transfer.TransferredBytes = transfer.TotalBytes;
                TransferCompleted?.Invoke(this, transfer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file transfer {TransferId}", transfer.Id);
            transfer.Status = FileTransferStatus.Failed;
            transfer.ErrorMessage = ex.Message;
            TransferCompleted?.Invoke(this, transfer);
        }
    }
}

