using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for file transfer operations.
/// </summary>
public interface IFileTransferService
{
    Task<FileTransferInfo> UploadFileAsync(int connectionProfileId, string localPath, string remotePath, CancellationToken ct = default);
    Task<FileTransferInfo> DownloadFileAsync(int connectionProfileId, string remotePath, string localPath, CancellationToken ct = default);
    Task<List<FileTransferInfo>> GetTransferHistoryAsync(int connectionProfileId, CancellationToken ct = default);
    Task PauseTransferAsync(int transferId, CancellationToken ct = default);
    Task ResumeTransferAsync(int transferId, CancellationToken ct = default);
    Task CancelTransferAsync(int transferId, CancellationToken ct = default);
    event EventHandler<FileTransferInfo>? TransferProgress;
    event EventHandler<FileTransferInfo>? TransferCompleted;
}

