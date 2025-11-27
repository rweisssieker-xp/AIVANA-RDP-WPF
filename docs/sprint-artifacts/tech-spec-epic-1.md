# Epic Technical Specification: Foundation & Project Setup

Date: 2025-11-27T09:33:21.548Z
Author: BMad
Epic ID: 1
Status: Draft

---

## Overview

Epic 1 establishes the technical foundation for Aivana_RDP_WPF, a modern Windows desktop RDP client application. This epic focuses on creating the project structure, configuring core infrastructure components, and setting up the development environment that will support all subsequent features. As outlined in the PRD, Aivana_RDP_WPF aims to transform the RDP client experience through superior UI/UX, comprehensive features, and performance optimization.

This foundation epic enables all future functionality by establishing the MVVM architecture pattern, dependency injection infrastructure, database persistence layer, logging framework, and configuration management system. The goal is to create a robust, maintainable codebase that follows modern .NET 8.0 and WPF best practices while integrating seamlessly with Windows APIs.

---

## Objectives and Scope

### In-Scope

- **Project Structure:** Create WPF project with MVVM folder organization (Views, ViewModels, Models, Services, Infrastructure, Commands, Converters, Helpers, Resources, Tests)
- **Core Dependencies:** Install and configure essential NuGet packages (Microsoft.Extensions.*, CommunityToolkit.Mvvm, Entity Framework Core)
- **MVVM Infrastructure:** Implement RelayCommand and AsyncRelayCommand classes, configure dependency injection container
- **Database Setup:** Configure SQLite with Entity Framework Core, create ApplicationDbContext, define initial data models (ConnectionProfile, SessionHistory)
- **Service Infrastructure:** Define core service interfaces (IConnectionProfileService, IRdpConnectionService, IFileTransferService, IClipboardService, ISessionRecordingService, IPerformanceMonitorService, ICredentialService, INotificationService)
- **Logging Infrastructure:** Configure Microsoft.Extensions.Logging with file logging provider, structured logging patterns
- **Configuration Management:** Set up appsettings.json, ApplicationSettings model, IOptions<T> pattern

### Out-of-Scope

- **Feature Implementation:** No actual RDP connection, file transfer, or UI features (covered in subsequent epics)
- **UI Components:** No Views or ViewModels beyond basic MainWindow (UI implementation in Epic 4)
- **RDP Integration:** No MSTSC ActiveX control integration (Epic 3)
- **Windows API Integration:** No Windows Credential Manager or other Windows API calls (Epic 2)
- **Testing:** No unit tests or integration tests (testing infrastructure setup only)

---

## System Architecture Alignment

This epic aligns with the Architecture Specification's foundational decisions:

- **Framework:** .NET 8.0 with WPF (ADR-001: WPF over WinUI 3)
- **Architecture Pattern:** MVVM pattern (ADR-002) with clear separation of Views, ViewModels, and Models
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection (Architecture section "Technology Stack Details")
- **Database:** SQLite with Entity Framework Core 8.0 (ADR-003)
- **Logging:** Microsoft.Extensions.Logging with structured logging (Architecture section "Logging Strategy")
- **Configuration:** appsettings.json + User Settings pattern (Architecture section "Configuration")

The project structure follows the Architecture Specification's "Project Structure" section exactly, establishing the folder hierarchy that will house all future features. Service interfaces defined in this epic match the Architecture's "API Contracts" section, ensuring consistency across the application.

---

## Detailed Design

### Services and Modules

| Service/Module | Responsibility | Inputs | Outputs | Owner |
|----------------|----------------|--------|---------|-------|
| **Dependency Injection Container** | Service registration and resolution | Service registrations | Resolved service instances | App.xaml.cs / ServiceConfiguration.cs |
| **ApplicationDbContext** | Database context for EF Core | DbSet<T> entities | Database operations | Infrastructure/Database/ |
| **IConnectionProfileService** | Connection profile business logic interface | ConnectionProfile operations | Task<ConnectionProfile>, Task<List<ConnectionProfile>> | Services/ |
| **IRdpConnectionService** | RDP connection management interface | ConnectionProfile, CancellationToken | Task, Connection events | Services/ |
| **IFileTransferService** | File transfer operations interface | File paths, session ID | Task, Transfer progress events | Services/ |
| **IClipboardService** | Clipboard synchronization interface | Clipboard data | Task | Services/ |
| **ISessionRecordingService** | Session recording interface | Recording settings | Task, Recording events | Services/ |
| **IPerformanceMonitorService** | Performance metrics collection interface | Connection ID | Task<PerformanceMetrics> | Services/ |
| **ICredentialService** | Credential management interface | Credential operations | Task | Services/ |
| **INotificationService** | User notifications interface | Notification data | Task | Services/ |
| **ILogger<T>** | Structured logging | Log messages, parameters | Log entries to file/console | Microsoft.Extensions.Logging |
| **IOptions<ApplicationSettings>** | Configuration access | Configuration key | ApplicationSettings instance | Microsoft.Extensions.Options |

### Data Models and Contracts

**ConnectionProfile Entity:**
```csharp
public class ConnectionProfile
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ServerAddress { get; set; }
    public int Port { get; set; } = 3389;
    public string Username { get; set; }
    public string? Domain { get; set; }
    public bool IsFavorite { get; set; }
    public string? GroupName { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Settings { get; set; } // JSON string
    public DateTime CreatedAt { get; set; }
    public DateTime? LastConnectedAt { get; set; }
    public int ConnectionCount { get; set; }
}
```

**SessionHistory Entity:**
```csharp
public class SessionHistory
{
    public int Id { get; set; }
    public int ConnectionProfileId { get; set; }
    public ConnectionProfile ConnectionProfile { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public SessionStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}
```

**ApplicationSettings Model:**
```csharp
public class ApplicationSettings
{
    public DefaultConnectionSettings DefaultConnection { get; set; }
    public UIPreferences UI { get; set; }
    public UpdatePreferences Updates { get; set; }
    public PerformanceSettings Performance { get; set; }
}
```

**Database Schema:**
- **ConnectionProfiles Table:** Id (PK), Name, ServerAddress, Port, Username, Domain, IsFavorite, GroupName, Tags (JSON), Settings (JSON), CreatedAt, LastConnectedAt, ConnectionCount
- **SessionHistory Table:** Id (PK), ConnectionProfileId (FK), ConnectedAt, DisconnectedAt, Duration, Status, ErrorMessage
- **Indexes:** ConnectionProfiles on IsFavorite and GroupName; SessionHistory on ConnectionProfileId and ConnectedAt

### APIs and Interfaces

**Service Interface Pattern:**
All service interfaces follow async/await pattern with CancellationToken support:

```csharp
public interface IConnectionProfileService
{
    Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken cancellationToken = default);
    Task<ConnectionProfile?> GetProfileByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConnectionProfile> CreateProfileAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(ConnectionProfile profile, CancellationToken cancellationToken = default);
    Task DeleteProfileAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ConnectionProfile>> GetFavoritesAsync(CancellationToken cancellationToken = default);
    Task<List<ConnectionProfile>> GetByGroupAsync(string groupName, CancellationToken cancellationToken = default);
}
```

**Dependency Injection Registration:**
```csharp
services.AddSingleton<IConnectionProfileService, ConnectionProfileService>();
services.AddSingleton<IRdpConnectionService, RdpConnectionService>();
services.AddSingleton<IFileTransferService, FileTransferService>();
services.AddSingleton<IClipboardService, ClipboardService>();
services.AddSingleton<ISessionRecordingService, SessionRecordingService>();
services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();
services.AddSingleton<ICredentialService, CredentialService>();
services.AddSingleton<INotificationService, NotificationService>();
```

### Workflows and Sequencing

**Project Initialization Sequence:**
1. Create WPF project: `dotnet new wpf -n Aivana_RDP_WPF -f net8.0`
2. Install NuGet packages (Microsoft.Extensions.*, CommunityToolkit.Mvvm, EF Core)
3. Create folder structure (Views, ViewModels, Models, Services, Infrastructure, Commands, Converters, Helpers, Resources, Tests)
4. Configure App.xaml.cs for dependency injection
5. Create ApplicationDbContext and initial data models
6. Create EF Core migration and apply to database
7. Define service interfaces in Services folder
8. Configure logging in App.xaml.cs or LoggerConfiguration.cs
9. Create appsettings.json and ApplicationSettings model
10. Register all services in DI container

**Service Registration Flow:**
```
App.xaml.cs OnStartup
  → ConfigureServices()
    → AddDbContext<ApplicationDbContext>()
    → AddLogging()
    → AddConfiguration()
    → Register Service Interfaces → Implementations
    → Build ServiceProvider
    → Set MainWindow.DataContext with injected ViewModel
```

**Database Initialization Flow:**
```
Application Startup
  → ApplicationDbContext created via DI
  → EF Core checks for pending migrations
  → If migrations exist, apply automatically
  → Database file created at %AppData%\Aivana_RDP_WPF\aivana.db
```

---

## Non-Functional Requirements

### Performance

**Startup Performance (NFR1):**
- Application startup time must be under 2 seconds on systems with SSD and 8GB+ RAM
- Lazy-load services where possible
- Defer heavy initialization until first use
- Database connection pooling for EF Core

**Memory Footprint (NFR2):**
- Memory footprint must not exceed 150MB for typical usage
- Use dependency injection singleton pattern appropriately
- Dispose resources properly (IDisposable pattern)
- Limit in-memory caching

**CPU Usage (NFR3):**
- CPU usage must remain under 5% when idle
- Use async/await for all I/O operations
- Avoid blocking UI thread
- Background tasks use Task.Run or background services

**UI Responsiveness (NFR8):**
- UI responsiveness must maintain 60 FPS during window resizing and animations
- Use Dispatcher.InvokeAsync for UI updates from background threads
- Hardware acceleration enabled (RenderOptions.ProcessRenderMode)

### Security

**Credential Storage (NFR11, NFR16):**
- All credentials must be encrypted at rest using Windows Credential Manager
- Credential storage must be protected against unauthorized access
- No plain-text credential storage in database or configuration files
- Windows security model provides encryption and access control

**Data Protection:**
- Connection profiles stored in SQLite (local file, user-specific)
- Application settings stored in user AppData folder
- Log files exclude sensitive data (no passwords, credentials in logs)

**Security Best Practices (NFR15):**
- Follow Windows security best practices
- Use secure coding patterns (input validation, parameterized queries)
- No hardcoded secrets or credentials

### Reliability/Availability

**Database Reliability:**
- SQLite database file stored in user AppData folder
- Automatic migration on application startup
- Database backup strategy (copy database file)
- Handle database corruption gracefully

**Error Handling:**
- All service methods handle exceptions gracefully
- Log errors with context using ILogger
- Show user-friendly error messages via InfoBar or ContentDialog
- Retry logic for transient failures (exponential backoff)

**Application Resilience:**
- Application handles missing configuration gracefully
- Default values provided for all settings
- Graceful degradation if optional services unavailable

### Observability

**Logging Requirements:**
- Structured logging with Microsoft.Extensions.Logging
- Log levels: Trace, Debug, Information, Warning, Error, Critical
- Log format: `_logger.LogInformation("Operation {Operation} on {Entity}", operation, entity)`
- Log files: `%AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log`
- Console logging enabled for development
- Log retention: 30 days (configurable)

**Metrics:**
- Application startup time
- Service initialization time
- Database operation duration
- Memory usage tracking

**Tracing:**
- Operation context tracking (correlation IDs)
- Service method entry/exit logging
- Exception stack traces logged

---

## Dependencies and Integrations

### NuGet Package Dependencies

**Core Framework:**
- `Microsoft.NET.Sdk` (8.0.0) - .NET SDK
- `Microsoft.WindowsDesktop.App` (8.0.0) - WPF framework

**Dependency Injection:**
- `Microsoft.Extensions.DependencyInjection` (8.0.0) - DI container
- `Microsoft.Extensions.DependencyInjection.Abstractions` (8.0.0) - DI abstractions

**Logging:**
- `Microsoft.Extensions.Logging` (8.0.0) - Logging framework
- `Microsoft.Extensions.Logging.Console` (8.0.0) - Console logging provider
- `Microsoft.Extensions.Logging.Debug` (8.0.0) - Debug logging provider

**Configuration:**
- `Microsoft.Extensions.Configuration` (8.0.0) - Configuration framework
- `Microsoft.Extensions.Configuration.Json` (8.0.0) - JSON configuration provider
- `Microsoft.Extensions.Options` (8.0.0) - Options pattern

**MVVM:**
- `CommunityToolkit.Mvvm` (8.2.2) - MVVM helpers (INotifyPropertyChanged, ICommand)

**Database:**
- `Microsoft.EntityFrameworkCore.Sqlite` (8.0.0) - SQLite EF Core provider
- `Microsoft.EntityFrameworkCore.Tools` (8.0.0) - EF Core tools (migrations)

**Windows Integration:**
- `Microsoft.Windows.SDK.Contracts` (10.0.26100.1) - Windows SDK contracts

**Testing (for test project):**
- `xunit` (2.6.2) - Unit testing framework
- `xunit.runner.visualstudio` (2.5.4) - Test runner
- `Moq` (4.20.70) - Mocking framework
- `FluentAssertions` (6.12.0) - Fluent assertions

### System Dependencies

- **Windows 10/11:** Required OS platform
- **.NET 8.0 Runtime:** Required runtime (bundled with installer)
- **SQLite:** Included with EF Core SQLite provider
- **Windows Credential Manager API:** Native Windows API (no NuGet package needed)

### Integration Points

- **Windows File System:** User AppData folder for database and logs
- **Windows Registry:** (Optional) Application settings storage
- **Windows Event Log:** (Optional) Enterprise logging

---

## Acceptance Criteria (Authoritative)

1. **AC1:** WPF project created with .NET 8.0 target framework, builds successfully without errors
2. **AC2:** Project folder structure matches Architecture Specification: Views/, ViewModels/, Models/, Services/, Infrastructure/, Commands/, Converters/, Helpers/, Resources/, Tests/
3. **AC3:** Core NuGet packages installed: Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging, Microsoft.Extensions.Configuration, CommunityToolkit.Mvvm, Microsoft.EntityFrameworkCore.Sqlite
4. **AC4:** Dependency injection container configured in App.xaml.cs, services can be resolved via constructor injection
5. **AC5:** RelayCommand<T> and AsyncRelayCommand<T> classes implemented in Commands folder
6. **AC6:** ApplicationDbContext created in Infrastructure/Database folder, inherits from DbContext
7. **AC7:** ConnectionProfile and SessionHistory entities defined in Models folder with all required properties
8. **AC8:** Initial EF Core migration created and applied, database file created at %AppData%\Aivana_RDP_WPF\aivana.db
9. **AC9:** All core service interfaces defined: IConnectionProfileService, IRdpConnectionService, IFileTransferService, IClipboardService, ISessionRecordingService, IPerformanceMonitorService, ICredentialService, INotificationService
10. **AC10:** Service interfaces registered in DI container with implementations (can be stubs initially)
11. **AC11:** ILogger<T> can be injected into services and ViewModels
12. **AC12:** Log files written to %AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log
13. **AC13:** Console logging enabled for development environment
14. **AC14:** Structured logging pattern used: `_logger.LogInformation("Message {Parameter}", value)`
15. **AC15:** appsettings.json exists in project root with default configuration
16. **AC16:** appsettings.Development.json exists for development overrides
17. **AC17:** ApplicationSettings model exists in Models folder
18. **AC18:** IOptions<ApplicationSettings> pattern used for settings access
19. **AC19:** User settings can be stored in %AppData%\Aivana_RDP_WPF\appsettings.json
20. **AC20:** Application launches to blank MainWindow successfully
21. **AC21:** Application startup time under 2 seconds on standard hardware (NFR1)

---

## Traceability Mapping

| AC | Spec Section | Component(s)/API(s) | Test Idea |
|----|--------------|---------------------|-----------|
| AC1 | Project Structure | Aivana_RDP_WPF.csproj | Verify project builds, target framework is net8.0 |
| AC2 | Project Structure | Folder structure | Verify all folders exist per Architecture spec |
| AC3 | Dependencies | .csproj PackageReference | Verify packages installed via dotnet list package |
| AC4 | MVVM Infrastructure | App.xaml.cs, IServiceCollection | Unit test: Service can be resolved from DI container |
| AC5 | Commands | Commands/RelayCommand.cs, Commands/AsyncRelayCommand.cs | Unit test: RelayCommand executes action, AsyncRelayCommand executes async action |
| AC6 | Database Setup | Infrastructure/Database/ApplicationDbContext.cs | Verify ApplicationDbContext inherits from DbContext |
| AC7 | Data Models | Models/ConnectionProfile.cs, Models/SessionHistory.cs | Verify entities have all required properties, EF Core recognizes them |
| AC8 | Database Setup | EF Core Migrations | Verify migration created, database file exists after migration apply |
| AC9 | Service Infrastructure | Services/*.cs interfaces | Verify all interfaces exist with async method signatures |
| AC10 | Service Infrastructure | App.xaml.cs service registration | Verify services registered, can be resolved |
| AC11 | Logging Infrastructure | ILogger<T> injection | Unit test: Service receives ILogger via constructor injection |
| AC12 | Logging Infrastructure | LoggerConfiguration.cs | Verify log file created in correct location |
| AC13 | Logging Infrastructure | LoggerConfiguration.cs | Verify console logging enabled in development |
| AC14 | Logging Strategy | Service logging calls | Verify structured logging pattern used in code |
| AC15 | Configuration | appsettings.json | Verify file exists with valid JSON structure |
| AC16 | Configuration | appsettings.Development.json | Verify file exists, can override appsettings.json |
| AC17 | Configuration | Models/ApplicationSettings.cs | Verify model exists with required properties |
| AC18 | Configuration | IOptions<ApplicationSettings> | Unit test: IOptions resolves ApplicationSettings |
| AC19 | Configuration | User settings file | Verify user settings file can be read/written |
| AC20 | Application Launch | MainWindow.xaml | Verify application launches, MainWindow displays |
| AC21 | Performance | Application startup | Performance test: Measure startup time, verify < 2 seconds |

---

## Risks, Assumptions, Open Questions

### Risks

**Risk 1: EF Core Migration Failures**
- **Description:** Database migrations may fail if database file is locked or corrupted
- **Mitigation:** Implement migration retry logic, handle exceptions gracefully, provide user-friendly error messages
- **Next Step:** Test migration failure scenarios, implement error handling

**Risk 2: Performance Impact of Dependency Injection**
- **Description:** Service resolution overhead may impact startup time
- **Mitigation:** Use singleton pattern for stateless services, lazy initialization where appropriate
- **Next Step:** Profile startup performance, optimize service registration order

**Risk 3: Configuration File Conflicts**
- **Description:** User settings file may conflict with default appsettings.json
- **Mitigation:** Implement configuration hierarchy (default → user override), validate configuration on load
- **Next Step:** Test configuration override scenarios, implement validation

### Assumptions

**Assumption 1:** Windows 10/11 with .NET 8.0 Runtime available on target systems
- **Rationale:** PRD specifies Windows 10/11 platform support
- **Validation:** Verify runtime availability during installation

**Assumption 2:** User has write access to %AppData% folder
- **Rationale:** Standard Windows user permissions
- **Validation:** Handle permission errors gracefully, provide user-friendly error messages

**Assumption 3:** SQLite database file can be created in user AppData folder
- **Rationale:** Standard desktop application pattern
- **Validation:** Test database creation in various Windows environments

### Open Questions

**Question 1:** Should database migrations run automatically on startup or require user confirmation?
- **Recommendation:** Automatic migration on startup (standard EF Core pattern)
- **Next Step:** Implement automatic migration, add logging for migration events

**Question 2:** Should logging be configurable via appsettings.json or hardcoded?
- **Recommendation:** Configurable via appsettings.json (log levels, file paths)
- **Next Step:** Implement logging configuration from appsettings.json

**Question 3:** Should service implementations be stubs initially or full implementations?
- **Recommendation:** Stub implementations initially, full implementation in feature epics
- **Next Step:** Create stub service implementations with NotImplementedException for methods

---

## Test Strategy Summary

### Test Levels

**Unit Tests:**
- Test service interface definitions (compile-time checks)
- Test DI container registration and resolution
- Test ApplicationDbContext configuration
- Test ApplicationSettings model serialization/deserialization
- Test logging configuration and log file creation
- Test configuration loading from appsettings.json

**Integration Tests:**
- Test database creation and migration application
- Test EF Core entity mapping and relationships
- Test service registration and dependency resolution
- Test configuration hierarchy (default → user override)

**System Tests:**
- Test application startup and MainWindow display
- Test application startup performance (NFR1: < 2 seconds)
- Test memory footprint on startup (NFR2: < 150MB)
- Test CPU usage when idle (NFR3: < 5%)

### Test Frameworks

- **xUnit:** Unit and integration test framework
- **Moq:** Mocking framework for service dependencies
- **FluentAssertions:** Fluent assertion library for readable tests

### Coverage Targets

- **Service Interfaces:** 100% interface definition coverage
- **DI Configuration:** 100% service registration coverage
- **Data Models:** 100% entity property coverage
- **Configuration:** 100% settings model coverage

### Critical Test Scenarios

1. **Application Startup:** Verify application launches successfully, MainWindow displays
2. **Database Initialization:** Verify database created, migrations applied
3. **Service Resolution:** Verify all services can be resolved from DI container
4. **Logging:** Verify log files created, structured logging works
5. **Configuration:** Verify appsettings.json loaded, user settings override works
6. **Performance:** Verify startup time < 2 seconds, memory < 150MB

---

_This technical specification provides the foundation for Epic 1 implementation. All subsequent epics will build upon this infrastructure._

