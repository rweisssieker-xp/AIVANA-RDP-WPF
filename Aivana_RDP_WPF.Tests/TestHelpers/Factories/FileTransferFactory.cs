using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Factories;

/// <summary>
/// Factory for creating test FileTransferInfo instances
/// </summary>
public static class FileTransferFactory
{
    private static int _idCounter = 1;

    public static FileTransferInfo Create(
        int connectionProfileId = 1,
        string localPath = @"C:\test\file.txt",
        string remotePath = @"\\remote\file.txt",
        FileTransferDirection direction = FileTransferDirection.Upload,
        FileTransferStatus status = FileTransferStatus.InProgress,
        long totalBytes = 1024 * 1024, // 1 MB default
        double progress = 0.0)
    {
        return new FileTransferInfo
        {
            Id = _idCounter++,
            ConnectionProfileId = connectionProfileId,
            LocalPath = localPath,
            RemotePath = remotePath,
            Direction = direction,
            Status = status,
            TotalBytes = totalBytes,
            TransferredBytes = (long)(totalBytes * progress),
            StartedAt = DateTime.UtcNow,
            CompletedAt = status == FileTransferStatus.Completed ? DateTime.UtcNow : null
        };
    }

    public static List<FileTransferInfo> CreateMultiple(
        int connectionProfileId = 1,
        int count = 5)
    {
        var transfers = new List<FileTransferInfo>();
        for (int i = 1; i <= count; i++)
        {
            transfers.Add(Create(
                connectionProfileId: connectionProfileId,
                localPath: $@"C:\test\test-file-{i}.txt",
                remotePath: $@"\\remote\test-file-{i}.txt",
                totalBytes: 1024 * 1024 * i, // Increasing file sizes
                progress: i * 0.2 // Varying progress
            ));
        }
        return transfers;
    }
}

