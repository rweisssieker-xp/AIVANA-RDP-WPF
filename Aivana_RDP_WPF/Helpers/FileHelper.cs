using System.IO;

namespace Aivana_RDP_WPF.Helpers;

/// <summary>
/// Helper class for file operations.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Ensures a directory exists, creating it if necessary.
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// Gets the application data directory.
    /// </summary>
    public static string GetAppDataDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "Aivana_RDP_WPF");
    }

    /// <summary>
    /// Gets the logs directory.
    /// </summary>
    public static string GetLogsDirectory()
    {
        return Path.Combine(GetAppDataDirectory(), "Logs");
    }

    /// <summary>
    /// Gets the thumbnails directory.
    /// </summary>
    public static string GetThumbnailsDirectory()
    {
        return Path.Combine(GetAppDataDirectory(), "Thumbnails");
    }

    /// <summary>
    /// Gets the recordings directory.
    /// </summary>
    public static string GetRecordingsDirectory()
    {
        return Path.Combine(GetAppDataDirectory(), "Recordings");
    }
}

