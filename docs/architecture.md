# Aivana_RDP_WPF - Architecture Specification

**Author:** BMad  
**Date:** 2025-11-26T16:39:36.075Z  
**Version:** 1.0

---

## Executive Summary

Aivana_RDP_WPF is architected as a modern Windows desktop application using WPF (Windows Presentation Foundation) with .NET 8.0, following MVVM (Model-View-ViewModel) pattern for separation of concerns. The architecture emphasizes performance through hardware-accelerated rendering, async/await patterns for all I/O operations, and modular design enabling independent feature development. The application integrates deeply with Windows APIs for credential management, file operations, and system notifications while maintaining a clean separation between UI, business logic, and RDP protocol handling.

---

## Project Initialization

**No standard starter template available for WPF desktop applications.**

Project initialization should be done manually:

```bash
# Create new WPF project
dotnet new wpf -n Aivana_RDP_WPF -f net8.0

# Add required NuGet packages (see Technology Stack Details section)
```

**First implementation story should execute:**
1. Create WPF project structure
2. Install core NuGet packages
3. Set up MVVM infrastructure (ViewModels, Commands, Services)
4. Configure dependency injection container
5. Set up project folder structure

---

## Decision Summary

| Category | Decision | Version | Affects FR Categories | Rationale |
| -------- | -------- | ------- | --------------------- | --------- |
| **Framework** | .NET / WPF | 8.0 | All | Native Windows desktop framework, optimal performance, Fluent Design support |
| **UI Framework** | WPF with Fluent Design | 2.0 | UI/UX | Native Windows 11 design language, consistent with system apps |
| **Architecture Pattern** | MVVM | - | All | Separation of concerns, testability, data binding support |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection | 8.0 | All | Built-in .NET DI, lightweight, well-supported |
| **RDP Protocol** | MSTSC ActiveX Control | - | Connection Management | Native Windows RDP support, no external dependencies |
| **Local Database** | SQLite with Entity Framework Core | 8.0 | Connection Management, Settings | Lightweight, file-based, no server required |
| **Async Pattern** | async/await | - | All I/O Operations | Non-blocking UI, responsive application |
| **Credential Storage** | Windows Credential Manager API | - | Security | Secure, OS-integrated credential storage |
| **File Transfer** | RDP Virtual Channels | - | File Transfer | Native RDP protocol extension |
| **Clipboard Sync** | RDP Clipboard Redirection | - | Clipboard Sync | Built-in RDP feature |
| **Session Recording** | Windows Media Foundation | - | Session Recording | Native Windows video encoding |
| **Logging** | Microsoft.Extensions.Logging | 8.0 | All | Structured logging, multiple providers |
| **Configuration** | appsettings.json + User Settings | - | Settings | Standard .NET configuration pattern |
| **Testing Framework** | xUnit + Moq | Latest | All | Industry standard for .NET testing |
| **Build Tool** | MSBuild (.NET SDK) | 8.0 | All | Built-in with .NET SDK |
| **Package Manager** | NuGet | Latest | All | Standard .NET package manager |

---

## Project Structure

```
Aivana_RDP_WPF/
├── Aivana_RDP_WPF.csproj
├── App.xaml
├── App.xaml.cs
├── appsettings.json
├── appsettings.Development.json
│
├── Views/                          # XAML Views (UI Layer)
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── ConnectionManagement/
│   │   ├── ConnectionListView.xaml
│   │   ├── ConnectionCardView.xaml
│   │   └── ConnectionConfigDialog.xaml
│   ├── SessionManagement/
│   │   ├── SessionTabView.xaml
│   │   └── MultiSessionDashboard.xaml
│   ├── FileTransfer/
│   │   ├── FileTransferView.xaml
│   │   └── TransferProgressView.xaml
│   ├── Performance/
│   │   └── PerformanceMonitorView.xaml
│   └── Settings/
│       └── SettingsView.xaml
│
├── ViewModels/                     # ViewModels (Presentation Logic)
│   ├── MainViewModel.cs
│   ├── ConnectionManagement/
│   │   ├── ConnectionListViewModel.cs
│   │   ├── ConnectionCardViewModel.cs
│   │   └── ConnectionConfigViewModel.cs
│   ├── SessionManagement/
│   │   ├── SessionViewModel.cs
│   │   └── MultiSessionViewModel.cs
│   ├── FileTransfer/
│   │   ├── FileTransferViewModel.cs
│   │   └── TransferProgressViewModel.cs
│   ├── Performance/
│   │   └── PerformanceMonitorViewModel.cs
│   └── Settings/
│       └── SettingsViewModel.cs
│
├── Models/                         # Domain Models
│   ├── ConnectionProfile.cs
│   ├── ConnectionGroup.cs
│   ├── SessionState.cs
│   ├── FileTransferItem.cs
│   ├── PerformanceMetrics.cs
│   └── ApplicationSettings.cs
│
├── Services/                       # Business Logic Services
│   ├── IRdpConnectionService.cs
│   ├── RdpConnectionService.cs
│   ├── IConnectionProfileService.cs
│   ├── ConnectionProfileService.cs
│   ├── IFileTransferService.cs
│   ├── FileTransferService.cs
│   ├── IClipboardService.cs
│   ├── ClipboardService.cs
│   ├── ISessionRecordingService.cs
│   ├── SessionRecordingService.cs
│   ├── IPerformanceMonitorService.cs
│   ├── PerformanceMonitorService.cs
│   ├── ICredentialService.cs
│   ├── CredentialService.cs
│   └── INotificationService.cs
│   └── NotificationService.cs
│
├── Infrastructure/                 # Infrastructure Layer
│   ├── Rdp/
│   │   ├── RdpClientWrapper.cs
│   │   ├── RdpVirtualChannel.cs
│   │   └── RdpEventHandlers.cs
│   ├── Database/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── IConnectionProfileRepository.cs
│   │   │   ├── ConnectionProfileRepository.cs
│   │   │   ├── ISessionHistoryRepository.cs
│   │   │   └── SessionHistoryRepository.cs
│   │   └── Migrations/
│   ├── Windows/
│   │   ├── CredentialManager.cs
│   │   ├── FileExplorerIntegration.cs
│   │   └── NotificationManager.cs
│   └── Logging/
│       └── LoggerConfiguration.cs
│
├── Commands/                       # ICommand Implementations
│   ├── RelayCommand.cs
│   ├── AsyncRelayCommand.cs
│   └── ConnectionCommands.cs
│
├── Converters/                     # Value Converters (XAML)
│   ├── BooleanToVisibilityConverter.cs
│   ├── ConnectionStatusToColorConverter.cs
│   └── DateTimeToRelativeTimeConverter.cs
│
├── Helpers/                        # Utility Classes
│   ├── ValidationHelper.cs
│   ├── FileHelper.cs
│   ├── NetworkHelper.cs
│   └── PerformanceHelper.cs
│
├── Resources/                      # Application Resources
│   ├── Styles/
│   │   ├── FluentDesignStyles.xaml
│   │   ├── ButtonStyles.xaml
│   │   └── CardStyles.xaml
│   ├── Themes/
│   │   ├── LightTheme.xaml
│   │   └── DarkTheme.xaml
│   ├── Icons/
│   └── Strings/
│       └── Resources.resx
│
├── Tests/                          # Unit Tests
│   ├── Aivana_RDP_WPF.Tests.csproj
│   ├── ViewModels/
│   │   └── ConnectionListViewModelTests.cs
│   ├── Services/
│   │   ├── ConnectionProfileServiceTests.cs
│   │   └── FileTransferServiceTests.cs
│   └── Infrastructure/
│       └── CredentialServiceTests.cs
│
└── Documentation/                  # Architecture & API Docs
    ├── Architecture.md
    ├── API.md
    └── Deployment.md
```

---

## FR Category to Architecture Mapping

| FR Category | Architecture Component | Location | Notes |
| ----------- | ----------------------- | -------- | ----- |
| **Connection Management** | ConnectionProfileService, ConnectionProfileRepository | Services/, Infrastructure/Database/ | SQLite stores profiles, Windows Credential Manager stores credentials |
| **User Interface & Experience** | Views/, ViewModels/, Resources/Styles/ | Views/, ViewModels/, Resources/ | Fluent Design styles, MVVM pattern |
| **File Transfer** | FileTransferService, RdpVirtualChannel | Services/, Infrastructure/Rdp/ | RDP Virtual Channels for file transfer |
| **Clipboard Synchronization** | ClipboardService, RDP Clipboard Redirection | Services/, Infrastructure/Rdp/ | Native RDP clipboard redirection |
| **Multi-Monitor Support** | RdpConnectionService | Services/ | RDP protocol multi-monitor configuration |
| **Session Recording** | SessionRecordingService, Windows Media Foundation | Services/, Infrastructure/ | WMF for video encoding |
| **Security & Authentication** | CredentialService, Windows Credential Manager | Services/, Infrastructure/Windows/ | OS-integrated secure storage |
| **Performance Monitoring** | PerformanceMonitorService | Services/ | Real-time metrics collection |
| **Settings & Configuration** | ApplicationSettings, appsettings.json | Models/, Root | JSON configuration + user settings |

---

## Technology Stack Details

### Core Technologies

**Framework:**
- **.NET 8.0** - Latest LTS version, optimal performance, modern C# features
- **WPF (Windows Presentation Foundation)** - Native Windows UI framework
- **C# 12** - Latest language features

**UI Framework:**
- **Fluent Design System 2.0** - Microsoft's design language for Windows 11
- **Windows Community Toolkit** - Additional Fluent Design controls
- **Microsoft.Xaml.Behaviors.Wpf** - Behaviors for XAML

**Architecture:**
- **MVVM Pattern** - Model-View-ViewModel separation
- **Microsoft.Extensions.DependencyInjection** - Dependency injection container
- **CommunityToolkit.Mvvm** - MVVM helpers (ICommand, INotifyPropertyChanged)

**RDP Protocol:**
- **MSTSC ActiveX Control** - Native Windows RDP client control (mstscax.dll)
- **RDP Virtual Channels** - Custom protocol extensions for file transfer
- **RDP Clipboard Redirection** - Built-in clipboard synchronization

**Data Persistence:**
- **SQLite** - Lightweight, file-based database
- **Entity Framework Core 8.0** - ORM for database access
- **Microsoft.EntityFrameworkCore.Sqlite** - SQLite provider

**Windows Integration:**
- **WindowsCredential API** - Windows Credential Manager integration
- **Windows Media Foundation** - Video encoding for session recording
- **Windows.Storage API** - File system operations
- **Windows.ApplicationModel.DataTransfer** - Clipboard operations

**Async & Concurrency:**
- **async/await** - Asynchronous programming pattern
- **System.Threading.Tasks** - Task-based asynchronous operations
- **System.Reactive** (optional) - Reactive extensions for event streams

**Logging:**
- **Microsoft.Extensions.Logging** - Structured logging framework
- **Serilog** (optional) - Advanced logging with structured data
- **NLog** (optional) - Alternative logging framework

**Configuration:**
- **Microsoft.Extensions.Configuration** - Configuration framework
- **Microsoft.Extensions.Configuration.Json** - JSON configuration provider
- **Microsoft.Extensions.Options** - Options pattern for settings

**Testing:**
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Fluent assertion library

### Integration Points

**RDP Connection Layer:**
- **MSTSC ActiveX Control** → Wrapped in `RdpClientWrapper`
- **RDP Events** → Handled by `RdpEventHandlers`, forwarded to ViewModels
- **Virtual Channels** → Implemented in `RdpVirtualChannel` for file transfer

**Windows Integration:**
- **Credential Manager** → `CredentialService` wraps Windows API calls
- **File Explorer** → `FileExplorerIntegration` handles drag-and-drop
- **Notifications** → `NotificationService` uses Windows Toast Notifications

**Data Layer:**
- **Entity Framework Core** → `ApplicationDbContext` manages database
- **Repositories** → Abstract data access, enable testing
- **Migrations** → Database schema versioning

**UI Layer:**
- **ViewModels** → Bind to Views via DataContext
- **Commands** → RelayCommand/AsyncRelayCommand for user actions
- **Converters** → Value converters for data transformation in XAML

---

## Implementation Patterns

These patterns ensure consistent implementation across all AI agents:

### Naming Conventions

**Classes:**
- ViewModels: `{Feature}ViewModel.cs` (e.g., `ConnectionListViewModel.cs`)
- Services: `{Feature}Service.cs` (e.g., `FileTransferService.cs`)
- Models: `{Entity}.cs` (e.g., `ConnectionProfile.cs`)
- Views: `{Feature}View.xaml` (e.g., `ConnectionListView.xaml`)

**Interfaces:**
- Prefix with `I`: `I{Feature}Service.cs` (e.g., `IFileTransferService.cs`)

**Namespaces:**
- `Aivana_RDP_WPF.{Layer}.{Feature}` (e.g., `Aivana_RDP_WPF.Services.FileTransfer`)

**Properties:**
- PascalCase: `ConnectionName`, `IsConnected`
- Boolean properties: Prefix with `Is`, `Has`, `Can` (e.g., `IsConnected`, `HasCredentials`)

**Methods:**
- PascalCase: `ConnectAsync()`, `SaveConnectionProfile()`
- Async methods: Suffix with `Async` (e.g., `ConnectAsync()`)

**Fields:**
- Private fields: `_camelCase` (e.g., `_connectionService`)
- Constants: `UPPER_CASE` (e.g., `MAX_CONNECTIONS`)

**Files:**
- One class per file
- File name matches class name exactly

### Code Organization

**Project Structure:**
- Separate folders for Views, ViewModels, Models, Services, Infrastructure
- Group related features in subfolders (e.g., `ConnectionManagement/`)
- Keep infrastructure concerns separate from business logic

**Dependency Direction:**
- Views → ViewModels (via DataContext)
- ViewModels → Services (via dependency injection)
- Services → Infrastructure (via interfaces)
- Models are independent (no dependencies)

**Service Registration:**
- Register all services in `App.xaml.cs` or dedicated `ServiceConfiguration.cs`
- Use `IServiceCollection` extension methods for organization
- Register interfaces with implementations

### Error Handling

**Exception Strategy:**
- Use specific exception types (e.g., `ConnectionException`, `FileTransferException`)
- Catch exceptions at service boundaries
- Log exceptions with context using `ILogger`
- Show user-friendly error messages via `InfoBar` or `ContentDialog`

**Error Handling Pattern:**
```csharp
try
{
    await _service.ConnectAsync(connection);
}
catch (ConnectionException ex)
{
    _logger.LogError(ex, "Failed to connect to {Server}", connection.Server);
    await _notificationService.ShowErrorAsync("Connection failed", ex.Message);
}
```

**Validation:**
- Validate input in ViewModels before calling services
- Use `INotifyDataErrorInfo` for property-level validation
- Show validation errors inline in UI

### Logging Strategy

**Logging Levels:**
- **Trace**: Detailed diagnostic information
- **Debug**: Development-time diagnostic information
- **Information**: General application flow
- **Warning**: Unexpected but recoverable situations
- **Error**: Errors that require attention
- **Critical**: Critical failures requiring immediate attention

**Logging Format:**
- Structured logging with parameters: `_logger.LogInformation("Connecting to {Server}", server)`
- Include context: Connection ID, User ID, Operation name
- Log exceptions with full stack trace

**Logging Locations:**
- Console (development)
- File (production)
- Windows Event Log (optional, for enterprise)

### Async/Await Patterns

**All I/O Operations:**
- Use `async/await` for all network, file, and database operations
- Return `Task` or `Task<T>` from async methods
- Use `ConfigureAwait(false)` in library code to avoid deadlocks

**UI Thread:**
- Use `Dispatcher.InvokeAsync()` when updating UI from background threads
- ViewModels automatically handle thread marshaling via `INotifyPropertyChanged`

**Cancellation:**
- Accept `CancellationToken` in all async methods
- Support cancellation for long-running operations
- Use `CancellationTokenSource` for user-initiated cancellation

### Data Binding Patterns

**ViewModel Properties:**
- Implement `INotifyPropertyChanged` (use `CommunityToolkit.Mvvm` helpers)
- Use `[ObservableProperty]` attribute for automatic property change notifications
- Raise `PropertyChanged` events for computed properties

**Commands:**
- Use `RelayCommand` for synchronous commands
- Use `AsyncRelayCommand` for async operations
- Enable/disable commands based on state (e.g., `CanConnect` property)

**Collections:**
- Use `ObservableCollection<T>` for collections that change
- Update collections on UI thread only

### State Management

**Application State:**
- Store in `ApplicationSettings` model
- Persist to `appsettings.json` or user settings
- Use `IOptions<T>` pattern for configuration

**Connection State:**
- Track in `SessionState` model
- Update via `RdpEventHandlers`
- Notify ViewModels via events or callbacks

**UI State:**
- Store in ViewModels (e.g., `IsLoading`, `SelectedConnection`)
- Use boolean flags for UI state (loading, error, success)

---

## Consistency Rules

### Naming Conventions

**REST-like Naming (for internal APIs):**
- Service methods: `Get{Entity}Async()`, `Create{Entity}Async()`, `Update{Entity}Async()`, `Delete{Entity}Async()`
- Example: `GetConnectionProfileAsync()`, `CreateConnectionProfileAsync()`

**Event Naming:**
- Events: `{Action}Completed`, `{Action}Failed` (e.g., `ConnectionCompleted`, `FileTransferFailed`)
- Event handlers: `On{Event}` (e.g., `OnConnectionCompleted`)

### Code Organization

**File Structure:**
- One class per file
- File name matches class name exactly
- Group related classes in folders

**Namespace Organization:**
- `Aivana_RDP_WPF.{Layer}` for main layers
- `Aivana_RDP_WPF.{Layer}.{Feature}` for feature-specific code

**Using Statements:**
- Group by: System, Microsoft, Third-party, Local
- Sort alphabetically within groups

### Format Patterns

**Date/Time Handling:**
- Use `DateTimeOffset` for all date/time values (timezone-aware)
- Store in UTC, display in user's local timezone
- Format: ISO 8601 for storage, user-friendly format for display

**API Response Format:**
- N/A (Desktop app, no REST API)
- Service methods return `Task<T>` or `Task` directly

**Error Format:**
- Custom exception types with `Message` property
- Include error code for programmatic handling
- User-friendly messages separate from technical details

### Communication Patterns

**Service Communication:**
- Services communicate via interfaces (dependency injection)
- Use events for cross-service notifications
- Avoid direct service-to-service dependencies

**ViewModel Communication:**
- ViewModels communicate via shared services
- Use `Messenger` pattern (CommunityToolkit.Mvvm) for loose coupling
- Pass data via constructor injection

### Lifecycle Patterns

**Loading States:**
- Use `IsLoading` boolean property in ViewModels
- Show `ProgressRing` in UI when `IsLoading == true`
- Disable actions during loading

**Error Recovery:**
- Show error message in `InfoBar`
- Provide retry button for recoverable errors
- Log errors for debugging

**Retries:**
- Implement retry logic in services (exponential backoff)
- Maximum 3 retries for network operations
- Show retry progress to user

### Location Patterns

**Configuration Files:**
- `appsettings.json` - Application configuration
- `appsettings.Development.json` - Development overrides
- User settings stored in `%AppData%\Aivana_RDP_WPF\`

**Database:**
- SQLite database: `%AppData%\Aivana_RDP_WPF\aivana.db`
- Migrations: `Infrastructure/Database/Migrations/`

**Logs:**
- Log files: `%AppData%\Aivana_RDP_WPF\Logs\`
- File naming: `aivana-{Date}.log`

---

## Data Architecture

### Data Models

**ConnectionProfile:**
```csharp
public class ConnectionProfile
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ServerAddress { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string? Domain { get; set; }
    public bool IsFavorite { get; set; }
    public string? GroupName { get; set; }
    public List<string> Tags { get; set; }
    public ConnectionSettings Settings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastConnectedAt { get; set; }
    public int ConnectionCount { get; set; }
}
```

**ConnectionGroup:**
```csharp
public class ConnectionGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public List<ConnectionProfile> Connections { get; set; }
}
```

**SessionHistory:**
```csharp
public class SessionHistory
{
    public int Id { get; set; }
    public int ConnectionProfileId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public SessionStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}
```

**FileTransferItem:**
```csharp
public class FileTransferItem
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string SourcePath { get; set; }
    public string DestinationPath { get; set; }
    public long FileSize { get; set; }
    public long BytesTransferred { get; set; }
    public TransferStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

**PerformanceMetrics:**
```csharp
public class PerformanceMetrics
{
    public int ConnectionId { get; set; }
    public double BandwidthMbps { get; set; }
    public int LatencyMs { get; set; }
    public double FrameRate { get; set; }
    public double PacketLoss { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Database Schema

**ConnectionProfiles Table:**
- Id (INTEGER PRIMARY KEY)
- Name (TEXT NOT NULL)
- ServerAddress (TEXT NOT NULL)
- Port (INTEGER DEFAULT 3389)
- Username (TEXT)
- Domain (TEXT)
- IsFavorite (INTEGER DEFAULT 0)
- GroupName (TEXT)
- Tags (TEXT) - JSON array
- Settings (TEXT) - JSON object
- CreatedAt (TEXT) - ISO 8601
- LastConnectedAt (TEXT) - ISO 8601, nullable
- ConnectionCount (INTEGER DEFAULT 0)

**SessionHistory Table:**
- Id (INTEGER PRIMARY KEY)
- ConnectionProfileId (INTEGER FOREIGN KEY)
- ConnectedAt (TEXT) - ISO 8601
- DisconnectedAt (TEXT) - ISO 8601, nullable
- Duration (TEXT) - ISO 8601 duration
- Status (TEXT) - Enum as string
- ErrorMessage (TEXT) - nullable

**Indexes:**
- ConnectionProfiles: Index on `IsFavorite`, `GroupName`
- SessionHistory: Index on `ConnectionProfileId`, `ConnectedAt`

---

## API Contracts

**N/A - Desktop Application**

This is a desktop application with no external REST API. Internal service contracts are defined via C# interfaces.

**Key Service Interfaces:**

```csharp
public interface IConnectionProfileService
{
    Task<List<ConnectionProfile>> GetAllProfilesAsync();
    Task<ConnectionProfile?> GetProfileByIdAsync(int id);
    Task<ConnectionProfile> CreateProfileAsync(ConnectionProfile profile);
    Task UpdateProfileAsync(ConnectionProfile profile);
    Task DeleteProfileAsync(int id);
    Task<List<ConnectionProfile>> GetFavoritesAsync();
    Task<List<ConnectionProfile>> GetByGroupAsync(string groupName);
}

public interface IRdpConnectionService
{
    Task ConnectAsync(ConnectionProfile profile, CancellationToken cancellationToken);
    Task DisconnectAsync(int sessionId);
    Task ReconnectAsync(int sessionId);
    event EventHandler<ConnectionEventArgs> ConnectionStateChanged;
    event EventHandler<PerformanceMetricsEventArgs> PerformanceMetricsUpdated;
}

public interface IFileTransferService
{
    Task TransferFileAsync(string sourcePath, string destinationPath, int sessionId, CancellationToken cancellationToken);
    Task TransferFilesAsync(List<string> sourcePaths, string destinationPath, int sessionId, CancellationToken cancellationToken);
    Task PauseTransferAsync(string transferId);
    Task ResumeTransferAsync(string transferId);
    event EventHandler<FileTransferProgressEventArgs> TransferProgress;
}
```

---

## Security Architecture

### Authentication

**Credential Storage:**
- Use Windows Credential Manager API (`CredWrite`, `CredRead`)
- Store credentials securely encrypted by Windows
- Credentials linked to connection profile ID
- No plain-text credential storage

**Credential Management:**
- `CredentialService` wraps Windows API calls
- Credentials stored with target: `Aivana_RDP_WPF:{ConnectionProfileId}`
- Username stored in connection profile (for display), password in Credential Manager

### Authorization

**Application-Level:**
- No user authentication required (single-user desktop app)
- All users have full access to their connection profiles
- Windows user account provides OS-level security

**Connection-Level:**
- RDP authentication handled by remote server
- Credentials provided from Windows Credential Manager
- Support for domain authentication, MFA (when server supports)

### Data Protection

**At Rest:**
- Connection profiles stored in SQLite (local file)
- Credentials stored in Windows Credential Manager (encrypted by OS)
- Application settings stored in user AppData folder

**In Transit:**
- RDP protocol uses TLS encryption (RDP security layer)
- File transfers use RDP Virtual Channels (encrypted)
- Clipboard sync uses RDP protocol encryption

### Audit Logging

**Logged Events:**
- Connection attempts (success/failure)
- Connection disconnections
- File transfer operations
- Credential access (read operations)
- Settings changes

**Log Format:**
- Structured logging with timestamp, event type, user context
- Logs stored in `%AppData%\Aivana_RDP_WPF\Logs\`
- Log retention: 30 days (configurable)

---

## Performance Considerations

### Rendering Performance

**Hardware Acceleration:**
- Enable GPU acceleration for WPF rendering
- Use `RenderOptions.ProcessRenderMode = RenderMode.Default` (GPU)
- Optimize XAML for rendering performance

**UI Responsiveness:**
- All I/O operations use async/await (non-blocking UI)
- Long-running operations show progress indicators
- Use `Dispatcher.InvokeAsync()` for UI updates from background threads

### Memory Management

**Connection Management:**
- Limit simultaneous connections (default: 10, configurable)
- Dispose RDP client controls when disconnected
- Clear session state on disconnect

**File Transfer:**
- Stream large files (don't load entire file into memory)
- Limit concurrent transfers (default: 3, configurable)
- Clear transfer queue after completion

### Network Performance

**RDP Optimization:**
- Adaptive quality based on network conditions
- Compression enabled for low-bandwidth connections
- Frame rate throttling for poor connections

**File Transfer:**
- Chunked transfer for large files
- Resume capability for interrupted transfers
- Bandwidth throttling option

---

## Deployment Architecture

### Application Distribution

**Installation Method:**
- MSI installer (Windows Installer)
- ClickOnce deployment (optional, for auto-updates)
- Portable executable (optional, for advanced users)

**Installation Location:**
- Program Files: `C:\Program Files\Aivana_RDP_WPF\`
- User Data: `%AppData%\Aivana_RDP_WPF\`
- Settings: `%AppData%\Aivana_RDP_WPF\appsettings.json`

### Update Strategy

**Auto-Update:**
- Check for updates on startup (configurable)
- Download updates in background
- Prompt user to install updates
- Rollback capability if update fails

**Update Mechanism:**
- Version check against update server
- Download MSI installer
- Silent install with user approval
- Restart application after update

### Prerequisites

**Runtime Requirements:**
- Windows 10 (version 1809+) or Windows 11
- .NET 8.0 Runtime (bundled with installer)
- RDP client components (included in Windows)

**Hardware Requirements:**
- CPU: x64 processor
- RAM: 4GB minimum, 8GB recommended
- Disk: 200MB for application, 500MB for data
- GPU: DirectX 9 compatible (for hardware acceleration)

---

## Development Environment

### Prerequisites

**Required Software:**
- Visual Studio 2022 (Community or higher)
- .NET 8.0 SDK
- Windows 10/11 development machine
- Git (for version control)

**Optional Tools:**
- Visual Studio Code (alternative IDE)
- JetBrains Rider (alternative IDE)
- SQLite Browser (for database inspection)

### Setup Commands

```bash
# Clone repository (when available)
git clone <repository-url>
cd Aivana_RDP_WPF

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project Aivana_RDP_WPF

# Run tests
dotnet test

# Create database migration
dotnet ef migrations add InitialCreate --project Aivana_RDP_WPF

# Apply migrations
dotnet ef database update --project Aivana_RDP_WPF
```

### NuGet Packages

**Core Packages:**
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

**Database:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

**Windows Integration:**
```xml
<PackageReference Include="Microsoft.Windows.SDK.Contracts" Version="10.0.26100.1" />
```

**Testing:**
```xml
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

---

## Architecture Decision Records (ADRs)

### ADR-001: WPF over WinUI 3

**Decision:** Use WPF instead of WinUI 3 for the UI framework.

**Rationale:**
- WPF is mature, stable, and well-documented
- Better third-party library support
- Easier migration path for existing RDP client code
- WinUI 3 is newer with less ecosystem support
- WPF supports Fluent Design through Windows Community Toolkit

**Alternatives Considered:**
- WinUI 3: Too new, limited ecosystem
- Electron: Performance overhead, larger footprint
- Avalonia: Cross-platform but less Windows integration

**Consequences:**
- Native Windows-only application
- Excellent performance and Windows integration
- Rich ecosystem of WPF libraries

---

### ADR-002: MVVM Pattern

**Decision:** Use MVVM (Model-View-ViewModel) pattern for architecture.

**Rationale:**
- Standard pattern for WPF applications
- Separation of concerns enables testability
- Data binding support in WPF
- CommunityToolkit.Mvvm provides excellent helpers

**Alternatives Considered:**
- MVC: Less suitable for WPF data binding
- Code-behind: Poor separation of concerns, hard to test

**Consequences:**
- Clear separation between UI and business logic
- ViewModels are unit-testable
- Requires more boilerplate code (mitigated by CommunityToolkit)

---

### ADR-003: SQLite for Local Storage

**Decision:** Use SQLite for connection profile and history storage.

**Rationale:**
- Lightweight, file-based database (no server required)
- Excellent .NET support via Entity Framework Core
- Sufficient for desktop application needs
- Easy backup (copy database file)

**Alternatives Considered:**
- JSON files: No query capabilities, harder to manage relationships
- XML files: Verbose, poor performance for large datasets
- PostgreSQL: Overkill for local desktop app

**Consequences:**
- Simple deployment (database file included)
- Easy migration and backup
- Limited to single-user scenarios (acceptable for desktop app)

---

### ADR-004: MSTSC ActiveX Control

**Decision:** Use native Windows MSTSC ActiveX Control for RDP connections.

**Rationale:**
- Native Windows RDP support, no external dependencies
- Optimal performance and compatibility
- Full RDP protocol feature support
- Well-integrated with Windows security

**Alternatives Considered:**
- FreeRDP: External dependency, potential compatibility issues
- Custom RDP implementation: Too complex, maintenance burden

**Consequences:**
- Windows-only solution (acceptable for Windows desktop app)
- Native performance and security
- Limited customization compared to open-source alternatives

---

### ADR-005: Windows Credential Manager

**Decision:** Use Windows Credential Manager API for secure credential storage.

**Rationale:**
- OS-integrated secure storage
- Encrypted by Windows, no custom encryption needed
- Familiar to Windows users
- No additional dependencies

**Alternatives Considered:**
- Encrypted SQLite: Custom encryption implementation, security risk
- Third-party credential managers: Additional dependencies, user friction

**Consequences:**
- Secure, OS-level credential protection
- Windows-only solution (acceptable)
- Users can manage credentials via Windows Credential Manager

---

_Generated by BMAD Decision Architecture Workflow v1.0_  
_Date: 2025-11-26T16:39:36.075Z_  
_For: BMad_


