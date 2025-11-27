using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for file transfer operations.
/// </summary>
public class FileTransferService : IFileTransferService
{
    private readonly ILogger<FileTransferService> _logger;

    public FileTransferService(ILogger<FileTransferService> logger)
    {
        _logger = logger;
    }
}

