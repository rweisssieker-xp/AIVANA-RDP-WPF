using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Factories;

/// <summary>
/// Factory for creating test FileTransfer instances
/// </summary>
public static class FileTransferFactory
{
    public static FileTransfer Create(
        Guid sessionId,
        string fileName,
        long fileSize = 1024 * 1024, // 1 MB default
        string status = "InProgress",
        double progress = 0.0)
    {
        return new FileTransfer
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            FileName = fileName,
            FileSize = fileSize,
            BytesTransferred = (long)(fileSize * progress),
            Status = status,
            StartedAt = DateTime.UtcNow,
            CompletedAt = status == "Completed" ? DateTime.UtcNow : null
        };
    }

    public static List<FileTransfer> CreateMultiple(
        Guid sessionId,
        int count = 5)
    {
        var transfers = new List<FileTransfer>();
        for (int i = 1; i <= count; i++)
        {
            transfers.Add(Create(
                sessionId,
                fileName: $"test-file-{i}.txt",
                fileSize: 1024 * 1024 * i, // Increasing file sizes
                progress: i * 0.2 // Varying progress
            ));
        }
        return transfers;
    }
}

