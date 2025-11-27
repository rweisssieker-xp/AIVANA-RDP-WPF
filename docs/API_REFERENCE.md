# Aivana RDP WPF - API Reference

**Version:** 1.0  
**Last Updated:** 2025-11-27

---

## Overview

This document provides API reference for Aivana RDP WPF services and key components.

---

## Services

### IConnectionProfileService

Manages connection profile CRUD operations.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### GetAllProfilesAsync

```csharp
Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default)
```

Retrieves all connection profiles from the database.

**Returns:** List of connection profiles ordered by name.

**Example:**
```csharp
var profiles = await _connectionProfileService.GetAllProfilesAsync();
```

---

#### GetProfileByIdAsync

```csharp
Task<ConnectionProfile?> GetProfileByIdAsync(int id, CancellationToken ct = default)
```

Retrieves a connection profile by ID.

**Parameters:**
- `id`: Connection profile ID

**Returns:** Connection profile or null if not found.

**Example:**
```csharp
var profile = await _connectionProfileService.GetProfileByIdAsync(1);
```

---

#### CreateProfileAsync

```csharp
Task<ConnectionProfile> CreateProfileAsync(ConnectionProfile profile, CancellationToken ct = default)
```

Creates a new connection profile.

**Parameters:**
- `profile`: Connection profile to create

**Returns:** Created connection profile with assigned ID.

**Example:**
```csharp
var newProfile = new ConnectionProfile
{
    Name = "Production Server",
    ServerAddress = "192.168.1.100",
    Port = 3389
};
var created = await _connectionProfileService.CreateProfileAsync(newProfile);
```

---

#### UpdateProfileAsync

```csharp
Task UpdateProfileAsync(ConnectionProfile profile, CancellationToken ct = default)
```

Updates an existing connection profile.

**Parameters:**
- `profile`: Connection profile with updated values

**Example:**
```csharp
profile.Name = "Updated Name";
await _connectionProfileService.UpdateProfileAsync(profile);
```

---

#### DeleteProfileAsync

```csharp
Task DeleteProfileAsync(int id, CancellationToken ct = default)
```

Deletes a connection profile.

**Parameters:**
- `id`: Connection profile ID to delete

**Example:**
```csharp
await _connectionProfileService.DeleteProfileAsync(1);
```

---

#### GetFavoritesAsync

```csharp
Task<List<ConnectionProfile>> GetFavoritesAsync(CancellationToken ct = default)
```

Retrieves all favorite connection profiles.

**Returns:** List of favorite profiles ordered by name.

---

#### GetByGroupAsync

```csharp
Task<List<ConnectionProfile>> GetByGroupAsync(string groupName, CancellationToken ct = default)
```

Retrieves connection profiles by group name.

**Parameters:**
- `groupName`: Group name to filter by

**Returns:** List of profiles in the specified group.

---

#### GetByTagAsync

```csharp
Task<List<ConnectionProfile>> GetByTagAsync(string tag, CancellationToken ct = default)
```

Retrieves connection profiles containing a specific tag.

**Parameters:**
- `tag`: Tag to search for

**Returns:** List of profiles with the specified tag.

---

#### ToggleFavoriteAsync

```csharp
Task ToggleFavoriteAsync(int profileId, CancellationToken ct = default)
```

Toggles the favorite status of a connection profile.

**Parameters:**
- `profileId`: Connection profile ID

---

#### GetAllGroupsAsync

```csharp
Task<List<string>> GetAllGroupsAsync(CancellationToken ct = default)
```

Retrieves all unique group names.

**Returns:** Sorted list of group names.

---

#### GetAllTagsAsync

```csharp
Task<List<string>> GetAllTagsAsync(CancellationToken ct = default)
```

Retrieves all unique tags across all profiles.

**Returns:** Sorted list of tag names.

---

### ICredentialService

Manages secure credential storage using Windows Credential Manager.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### SaveCredentialsAsync

```csharp
Task SaveCredentialsAsync(int connectionProfileId, string username, string password, CancellationToken ct = default)
```

Saves credentials for a connection profile in Windows Credential Manager.

**Parameters:**
- `connectionProfileId`: Connection profile ID
- `username`: Username
- `password`: Password (stored securely)

**Example:**
```csharp
await _credentialService.SaveCredentialsAsync(1, "admin", "password123");
```

---

#### GetCredentialsAsync

```csharp
Task<Credential?> GetCredentialsAsync(int connectionProfileId, CancellationToken ct = default)
```

Retrieves saved credentials for a connection profile.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Returns:** Credential object with username and password, or null if not found.

**Example:**
```csharp
var credential = await _credentialService.GetCredentialsAsync(1);
if (credential != null)
{
    var username = credential.Username;
    var password = credential.Password;
}
```

---

#### DeleteCredentialsAsync

```csharp
Task DeleteCredentialsAsync(int connectionProfileId, CancellationToken ct = default)
```

Deletes saved credentials for a connection profile.

**Parameters:**
- `connectionProfileId`: Connection profile ID

---

### IRdpConnectionService

Manages RDP connection lifecycle.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### CreateConnectionHost

```csharp
WindowsFormsHost? CreateConnectionHost(ConnectionProfile profile)
```

Creates a WindowsFormsHost containing the RDP ActiveX control.

**Parameters:**
- `profile`: Connection profile

**Returns:** WindowsFormsHost ready for embedding in WPF, or null on error.

**Example:**
```csharp
var host = _rdpConnectionService.CreateConnectionHost(profile);
// Add host to WPF UI
```

---

#### ConnectAsync

```csharp
Task ConnectAsync(ConnectionProfile profile, string? password = null)
```

Establishes an RDP connection.

**Parameters:**
- `profile`: Connection profile
- `password`: Optional password (if not using saved credentials)

**Example:**
```csharp
await _rdpConnectionService.ConnectAsync(profile, password);
```

---

#### Disconnect

```csharp
void Disconnect(int profileId)
```

Disconnects from an RDP session.

**Parameters:**
- `profileId`: Connection profile ID

---

#### IsConnected

```csharp
bool IsConnected(int profileId)
```

Checks if a connection is currently active.

**Parameters:**
- `profileId`: Connection profile ID

**Returns:** True if connected, false otherwise.

---

### IFileTransferService

Manages file transfer operations.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### UploadFileAsync

```csharp
Task<FileTransferInfo> UploadFileAsync(int connectionProfileId, string localPath, string remotePath, CancellationToken ct = default)
```

Uploads a file to the remote desktop.

**Parameters:**
- `connectionProfileId`: Connection profile ID
- `localPath`: Local file path
- `remotePath`: Remote destination path

**Returns:** FileTransferInfo with transfer details.

**Events:**
- `TransferProgress`: Fired during transfer with progress updates
- `TransferCompleted`: Fired when transfer completes

---

#### DownloadFileAsync

```csharp
Task<FileTransferInfo> DownloadFileAsync(int connectionProfileId, string remotePath, string localPath, CancellationToken ct = default)
```

Downloads a file from the remote desktop.

**Parameters:**
- `connectionProfileId`: Connection profile ID
- `remotePath`: Remote file path
- `localPath`: Local destination path

**Returns:** FileTransferInfo with transfer details.

---

#### GetTransferHistoryAsync

```csharp
Task<List<FileTransferInfo>> GetTransferHistoryAsync(int connectionProfileId, CancellationToken ct = default)
```

Retrieves transfer history for a connection.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Returns:** List of file transfers ordered by start time (newest first).

---

#### PauseTransferAsync

```csharp
Task PauseTransferAsync(int transferId, CancellationToken ct = default)
```

Pauses an active file transfer.

**Parameters:**
- `transferId`: Transfer ID

---

#### ResumeTransferAsync

```csharp
Task ResumeTransferAsync(int transferId, CancellationToken ct = default)
```

Resumes a paused file transfer.

**Parameters:**
- `transferId`: Transfer ID

---

#### CancelTransferAsync

```csharp
Task CancelTransferAsync(int transferId, CancellationToken ct = default)
```

Cancels an active file transfer.

**Parameters:**
- `transferId`: Transfer ID

---

### IClipboardService

Manages clipboard synchronization.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### SyncClipboardToRemoteAsync

```csharp
Task SyncClipboardToRemoteAsync(int connectionProfileId, CancellationToken ct = default)
```

Synchronizes local clipboard to remote session.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Events:**
- `ClipboardChanged`: Fired when clipboard content changes

---

#### SyncClipboardFromRemoteAsync

```csharp
Task SyncClipboardFromRemoteAsync(int connectionProfileId, CancellationToken ct = default)
```

Synchronizes remote clipboard to local machine.

**Parameters:**
- `connectionProfileId`: Connection profile ID

---

#### GetClipboardHistoryAsync

```csharp
Task<List<ClipboardItem>> GetClipboardHistoryAsync(int connectionProfileId, CancellationToken ct = default)
```

Retrieves clipboard history for a connection.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Returns:** List of clipboard items (last 50) ordered by time (newest first).

---

#### SetClipboardEnabledAsync

```csharp
Task SetClipboardEnabledAsync(int connectionProfileId, bool enabled, CancellationToken ct = default)
```

Enables or disables clipboard synchronization for a connection.

**Parameters:**
- `connectionProfileId`: Connection profile ID
- `enabled`: True to enable, false to disable

---

#### IsClipboardEnabled

```csharp
bool IsClipboardEnabled(int connectionProfileId)
```

Checks if clipboard synchronization is enabled.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Returns:** True if enabled, false otherwise.

---

### IPerformanceMonitorService

Monitors connection performance metrics.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### StartMonitoringAsync

```csharp
Task StartMonitoringAsync(int connectionProfileId, CancellationToken ct = default)
```

Starts performance monitoring for a connection.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Events:**
- `MetricsUpdated`: Fired every second with updated metrics

---

#### StopMonitoringAsync

```csharp
Task StopMonitoringAsync(int connectionProfileId, CancellationToken ct = default)
```

Stops performance monitoring for a connection.

**Parameters:**
- `connectionProfileId`: Connection profile ID

---

#### GetCurrentMetricsAsync

```csharp
Task<PerformanceMetrics?> GetCurrentMetricsAsync(int connectionProfileId, CancellationToken ct = default)
```

Retrieves current performance metrics.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Returns:** Current PerformanceMetrics or null if not monitoring.

---

#### GetHistoricalMetricsAsync

```csharp
Task<List<PerformanceMetrics>> GetHistoricalMetricsAsync(int connectionProfileId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
```

Retrieves historical performance metrics for a time range.

**Parameters:**
- `connectionProfileId`: Connection profile ID
- `startTime`: Start of time range
- `endTime`: End of time range

**Returns:** List of metrics ordered by timestamp.

---

### ISessionRecordingService

Manages session recording.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### StartRecordingAsync

```csharp
Task<SessionRecording> StartRecordingAsync(int connectionProfileId, string outputPath, CancellationToken ct = default)
```

Starts recording an RDP session.

**Parameters:**
- `connectionProfileId`: Connection profile ID
- `outputPath`: Path where recording will be saved

**Returns:** SessionRecording object with recording details.

**Events:**
- `RecordingStatusChanged`: Fired when recording status changes

---

#### StopRecordingAsync

```csharp
Task StopRecordingAsync(int recordingId, CancellationToken ct = default)
```

Stops a recording session.

**Parameters:**
- `recordingId`: Recording ID

---

#### PauseRecordingAsync

```csharp
Task PauseRecordingAsync(int recordingId, CancellationToken ct = default)
```

Pauses a recording session.

**Parameters:**
- `recordingId`: Recording ID

---

#### ResumeRecordingAsync

```csharp
Task ResumeRecordingAsync(int recordingId, CancellationToken ct = default)
```

Resumes a paused recording session.

**Parameters:**
- `recordingId`: Recording ID

---

#### GetRecordingsAsync

```csharp
Task<List<SessionRecording>> GetRecordingsAsync(int connectionProfileId, CancellationToken ct = default)
```

Retrieves all recordings for a connection.

**Parameters:**
- `connectionProfileId`: Connection profile ID

**Returns:** List of recordings ordered by start time (newest first).

---

#### DeleteRecordingAsync

```csharp
Task DeleteRecordingAsync(int recordingId, CancellationToken ct = default)
```

Deletes a recording and its file.

**Parameters:**
- `recordingId`: Recording ID

---

### IThemeService

Manages application themes.

**Namespace:** `Aivana_RDP_WPF.Services`

**Properties:**

#### CurrentTheme

```csharp
string CurrentTheme { get; }
```

Gets the current theme name ("Light" or "Dark").

**Events:**

#### ThemeChanged

```csharp
event EventHandler<string> ThemeChanged
```

Fired when theme changes.

**Methods:**

#### SetTheme

```csharp
void SetTheme(string themeName)
```

Sets the application theme.

**Parameters:**
- `themeName`: Theme name ("Light" or "Dark")

**Example:**
```csharp
_themeService.SetTheme("Dark");
```

---

#### GetAvailableThemes

```csharp
IEnumerable<string> GetAvailableThemes()
```

Gets list of available themes.

**Returns:** List of theme names.

---

### IImportExportService

Manages import/export of connection profiles.

**Namespace:** `Aivana_RDP_WPF.Services`

**Methods:**

#### ImportFromRdpFileAsync

```csharp
Task<IEnumerable<ConnectionProfile>> ImportFromRdpFileAsync(string filePath, CancellationToken ct = default)
```

Imports connection profiles from an RDP file.

**Parameters:**
- `filePath`: Path to .rdp file

**Returns:** List of imported connection profiles.

---

#### ImportFromJsonFileAsync

```csharp
Task<IEnumerable<ConnectionProfile>> ImportFromJsonFileAsync(string filePath, CancellationToken ct = default)
```

Imports connection profiles from a JSON file.

**Parameters:**
- `filePath`: Path to .json file

**Returns:** List of imported connection profiles.

---

#### ImportFromCsvFileAsync

```csharp
Task<IEnumerable<ConnectionProfile>> ImportFromCsvFileAsync(string filePath, CancellationToken ct = default)
```

Imports connection profiles from a CSV file.

**Parameters:**
- `filePath`: Path to .csv file

**Returns:** List of imported connection profiles.

---

#### ExportToRdpFileAsync

```csharp
Task ExportToRdpFileAsync(ConnectionProfile profile, string filePath, CancellationToken ct = default)
```

Exports a connection profile to an RDP file.

**Parameters:**
- `profile`: Connection profile to export
- `filePath`: Destination file path

---

#### ExportToJsonFileAsync

```csharp
Task ExportToJsonFileAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default)
```

Exports connection profiles to a JSON file.

**Parameters:**
- `profiles`: Connection profiles to export
- `filePath`: Destination file path

---

#### ExportToCsvFileAsync

```csharp
Task ExportToCsvFileAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default)
```

Exports connection profiles to a CSV file.

**Parameters:**
- `profiles`: Connection profiles to export
- `filePath`: Destination file path

---

## Models

### ConnectionProfile

Represents a connection profile.

**Namespace:** `Aivana_RDP_WPF.Models`

**Properties:**

- `Id` (int): Unique identifier
- `Name` (string): Connection name
- `ServerAddress` (string): Server IP or hostname
- `Port` (int): RDP port (default: 3389)
- `Username` (string?): Username
- `Domain` (string?): Windows domain
- `IsFavorite` (bool): Favorite status
- `GroupName` (string?): Group name
- `Tags` (string): JSON array of tags
- `Settings` (string): JSON object of connection settings
- `CreatedAt` (DateTime): Creation timestamp
- `LastConnectedAt` (DateTime?): Last connection timestamp
- `ConnectionCount` (int): Number of connections made

---

### FileTransferInfo

Represents a file transfer operation.

**Namespace:** `Aivana_RDP_WPF.Models`

**Properties:**

- `Id` (int): Transfer ID
- `ConnectionProfileId` (int): Associated connection profile
- `LocalPath` (string): Local file path
- `RemotePath` (string): Remote file path
- `Direction` (FileTransferDirection): Upload or Download
- `Status` (FileTransferStatus): Transfer status
- `TotalBytes` (long): Total file size
- `TransferredBytes` (long): Bytes transferred
- `StartedAt` (DateTime): Start timestamp
- `CompletedAt` (DateTime?): Completion timestamp
- `ErrorMessage` (string?): Error message if failed
- `ProgressPercentage` (double): Transfer progress (0-100)

---

### PerformanceMetrics

Represents connection performance metrics.

**Namespace:** `Aivana_RDP_WPF.Models`

**Properties:**

- `ConnectionProfileId` (int): Associated connection profile
- `Timestamp` (DateTime): Metric timestamp
- `LatencyMs` (double): Network latency in milliseconds
- `BandwidthMbps` (double): Available bandwidth in Mbps
- `PacketLossPercent` (int): Packet loss percentage
- `FrameRate` (double): Frames per second
- `QualityScore` (int): Quality score (0-100)
- `CpuUsagePercent` (double): CPU usage percentage
- `MemoryUsageMB` (double): Memory usage in MB
- `NetworkUsageMbps` (double): Network usage in Mbps

---

### ClipboardItem

Represents a clipboard item.

**Namespace:** `Aivana_RDP_WPF.Models`

**Properties:**

- `Id` (int): Clipboard item ID
- `ConnectionProfileId` (int): Associated connection profile
- `Format` (ClipboardFormat): Clipboard format (Text, Image, FileList, etc.)
- `Data` (byte[]): Clipboard data
- `CreatedAt` (DateTime): Creation timestamp
- `Preview` (string?): Text preview of content

---

### SessionRecording

Represents a session recording.

**Namespace:** `Aivana_RDP_WPF.Models`

**Properties:**

- `Id` (int): Recording ID
- `ConnectionProfileId` (int): Associated connection profile
- `FilePath` (string): Recording file path
- `StartedAt` (DateTime): Start timestamp
- `StoppedAt` (DateTime?): Stop timestamp
- `Status` (RecordingStatus): Recording status
- `FileSizeBytes` (long): File size in bytes
- `DurationSeconds` (int): Recording duration in seconds

---

## Helpers

### TagHelper

Utility for tag management (JSON serialization).

**Namespace:** `Aivana_RDP_WPF.Helpers`

**Methods:**

#### ParseTags

```csharp
static List<string> ParseTags(string tagsJson)
```

Parses JSON tags string to list of strings.

**Parameters:**
- `tagsJson`: JSON array string (e.g., `"[\"tag1\", \"tag2\"]"`)

**Returns:** List of tag strings.

---

#### SerializeTags

```csharp
static string SerializeTags(List<string> tags)
```

Serializes list of tags to JSON string.

**Parameters:**
- `tags`: List of tag strings

**Returns:** JSON array string.

---

#### HasTag

```csharp
static bool HasTag(string tagsJson, string tag)
```

Checks if tags JSON contains a specific tag.

**Parameters:**
- `tagsJson`: JSON tags string
- `tag`: Tag to search for

**Returns:** True if tag exists.

---

### ValidationHelper

Utility for input validation.

**Namespace:** `Aivana_RDP_WPF.Helpers`

**Methods:**

#### IsValidServerAddress

```csharp
static bool IsValidServerAddress(string address)
```

Validates server address (IP or hostname).

**Parameters:**
- `address`: Server address to validate

**Returns:** True if valid.

---

#### IsValidPort

```csharp
static bool IsValidPort(int port)
```

Validates port number.

**Parameters:**
- `port`: Port number to validate

**Returns:** True if valid (1-65535).

---

## Dependency Injection

Services are registered in `App.xaml.cs`:

```csharp
services.AddScoped<IConnectionProfileService, ConnectionProfileService>();
services.AddScoped<ICredentialService, CredentialService>();
services.AddScoped<IRdpConnectionService, RdpConnectionService>();
services.AddScoped<IFileTransferService, FileTransferService>();
services.AddScoped<IClipboardService, ClipboardService>();
services.AddScoped<IPerformanceMonitorService, PerformanceMonitorService>();
services.AddScoped<ISessionRecordingService, SessionRecordingService>();
services.AddSingleton<IThemeService, ThemeService>();
services.AddScoped<IImportExportService, ImportExportService>();
```

---

## Usage Examples

### Creating and Connecting

```csharp
// Create connection profile
var profile = new ConnectionProfile
{
    Name = "Production Server",
    ServerAddress = "192.168.1.100",
    Port = 3389,
    Username = "admin"
};

var created = await _connectionProfileService.CreateProfileAsync(profile);

// Save credentials
await _credentialService.SaveCredentialsAsync(created.Id, "admin", "password123");

// Create RDP host
var host = _rdpConnectionService.CreateConnectionHost(created);

// Connect
await _rdpConnectionService.ConnectAsync(created);
```

### Monitoring Performance

```csharp
// Start monitoring
await _performanceMonitorService.StartMonitoringAsync(profileId);

// Subscribe to updates
_performanceMonitorService.MetricsUpdated += (sender, metrics) =>
{
    Console.WriteLine($"Latency: {metrics.LatencyMs}ms");
    Console.WriteLine($"Bandwidth: {metrics.BandwidthMbps}Mbps");
};

// Get current metrics
var metrics = await _performanceMonitorService.GetCurrentMetricsAsync(profileId);
```

### File Transfer

```csharp
// Upload file
var transfer = await _fileTransferService.UploadFileAsync(
    profileId, 
    @"C:\local\file.txt", 
    @"C:\remote\file.txt"
);

// Subscribe to progress
_fileTransferService.TransferProgress += (sender, info) =>
{
    Console.WriteLine($"Progress: {info.ProgressPercentage}%");
};

// Wait for completion
_fileTransferService.TransferCompleted += (sender, info) =>
{
    Console.WriteLine($"Transfer completed: {info.Status}");
};
```

---

**Last Updated:** 2025-11-27

