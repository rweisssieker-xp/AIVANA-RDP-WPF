# Aivana RDP WPF — Complete Documentation

**Version:** 1.0  
**Date:** 2026-04-15  
**Framework:** .NET 8.0 · WPF · SQLite · MVVM

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [System Requirements](#2-system-requirements)
3. [Installation & Setup](#3-installation--setup)
4. [Application Architecture](#4-application-architecture)
5. [Feature Reference](#5-feature-reference)
   - 5.1 [Connection Profile Management](#51-connection-profile-management)
   - 5.2 [Multi-Protocol Support (RDP / SSH / VNC)](#52-multi-protocol-support-rdp--ssh--vnc)
   - 5.3 [Workspace & Session Management](#53-workspace--session-management)
   - 5.4 [Wake-on-LAN](#54-wake-on-lan)
   - 5.5 [SSH Tunnelling](#55-ssh-tunnelling)
   - 5.6 [Workflow Engine](#56-workflow-engine)
   - 5.7 [File Transfer](#57-file-transfer)
   - 5.8 [Clipboard Synchronisation](#58-clipboard-synchronisation)
   - 5.9 [Performance Monitoring](#59-performance-monitoring)
   - 5.10 [Session Recording](#510-session-recording)
   - 5.11 [Security & Credential Management](#511-security--credential-management)
   - 5.12 [Import / Export](#512-import--export)
   - 5.13 [Themes & UI Customisation](#513-themes--ui-customisation)
6. [Configuration Reference](#6-configuration-reference)
7. [Data Models](#7-data-models)
8. [Service Layer API](#8-service-layer-api)
9. [Keyboard Shortcuts](#9-keyboard-shortcuts)
10. [Logging & Diagnostics](#10-logging--diagnostics)
11. [Database](#11-database)
12. [Developer Guide](#12-developer-guide)
13. [Testing](#13-testing)
14. [Troubleshooting](#14-troubleshooting)
15. [Contributing](#15-contributing)
16. [Changelog Summary](#16-changelog-summary)

---

## 1. Project Overview

**Aivana RDP WPF** is an open-source, Windows-native remote desktop management application built with WPF and .NET 8. It goes far beyond the built-in Windows RDP client (mstsc.exe) by providing:

- **Multi-protocol connectivity** — RDP, SSH, and VNC from a single application
- **Workflow automation** — scriptless task automation for connection sequences
- **Wake-on-LAN** — wake sleeping machines before connecting
- **SSH Tunnel support** — connect securely through jump hosts
- **Persistent workspace** — tabbed interface with saveable layouts
- **File transfer, clipboard sync, session recording, and performance monitoring**

### Technology Stack at a Glance

| Layer | Technology |
|---|---|
| Runtime | .NET 8.0 |
| UI | WPF (Windows Presentation Foundation) |
| UI Pattern | MVVM — CommunityToolkit.Mvvm 8.2.2 |
| Database | SQLite — Entity Framework Core 8.0 |
| RDP | MSTSC ActiveX (mstscax.dll / MSTSCLib) |
| Credentials | Windows Credential Manager API |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Logging | Microsoft.Extensions.Logging |
| Config | Microsoft.Extensions.Configuration (JSON) |

---

## 2. System Requirements

### Minimum Requirements

| Requirement | Specification |
|---|---|
| **OS** | Windows 10 version 1809 or Windows 11 |
| **Runtime** | .NET 8.0 Runtime |
| **RAM** | 4 GB |
| **Disk** | 100 MB free |
| **CPU** | x64 processor |
| **Network** | Required for remote connections |

### Recommended Requirements

| Requirement | Specification |
|---|---|
| **OS** | Windows 11 |
| **RAM** | 8 GB or more |
| **Disk** | SSD with 500 MB free |
| **GPU** | DirectX 11 compatible (for hardware acceleration) |

### Software Dependencies

- `.NET 8.0 Runtime` — included in Windows 11; download separately for Windows 10
- `Visual C++ Redistributable` — usually pre-installed
- `mstscax.dll` — Windows system file for RDP rendering (present on all supported Windows versions)

---

## 3. Installation & Setup

### Option A — Build from Source

```bash
# 1. Clone the repository
git clone https://github.com/rweisssieker-xp/AIVANA-RDP-WPF.git
cd AIVANA-RDP-WPF

# 2. Restore NuGet packages
dotnet restore

# 3. Build
dotnet build Aivana_RDP_WPF.sln

# 4. Run database migrations (creates SQLite DB in %AppData%\Aivana_RDP_WPF\)
cd Aivana_RDP_WPF
dotnet ef database update

# 5. Run the application
dotnet run --project Aivana_RDP_WPF/Aivana_RDP_WPF.csproj
```

### Option B — Visual Studio

1. Open `Aivana_RDP_WPF.sln` in Visual Studio 2022
2. Select the **Debug** or **Release** configuration
3. Press **F5** (Run with debugging) or **Ctrl+F5** (Run without debugging)

### First Launch

On first launch the application:
1. Creates the SQLite database at `%AppData%\Aivana_RDP_WPF\aivana.db`
2. Applies any pending database migrations automatically
3. Shows an empty connection list — ready to use immediately

---

## 4. Application Architecture

### Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    WPF Views (XAML)                     │
│   MainWindow · ConnectionListView · MultiSessionView    │
│   ConnectionConfigDialog · SettingsView · ...           │
├─────────────────────────────────────────────────────────┤
│                   ViewModels (MVVM)                     │
│   MainViewModel · ConnectionListViewModel               │
│   MultiSessionViewModel · WorkspaceViewModel            │
│   WorkflowViewModel · SettingsViewModel · ...           │
├─────────────────────────────────────────────────────────┤
│                   Service Layer                         │
│   ConnectionProfileService  · RdpConnectionService      │
│   FileTransferService · ClipboardService                │
│   PerformanceMonitorService · SessionRecordingService   │
│   WorkflowEngineService · WorkspaceService              │
│   WakeOnLanService · SshTunnelService                   │
│   CredentialService · ThemeService · ...                │
├───────────────────┬─────────────────────────────────────┤
│   Infrastructure  │        Data Models                  │
│   ─────────────── │   ConnectionProfile                 │
│   Database/       │   RemoteSession / SessionHistory     │
│   ApplicationDb   │   PerformanceMetrics                │
│   Context         │   WorkflowDefinition / Execution    │
│                   │   WorkspaceTab / WorkspaceLayout     │
│   Rdp/            │   FileTransferInfo                  │
│   RdpClientWrapper│   ClipboardItem                     │
│                   │   SessionRecording                  │
│   Credentials/    │   SshTunnelConfiguration            │
│   WindowsCred     │   ApplicationSettings / AppSetting  │
│   Manager         │   ConnectionSuggestion · WakeResult │
│                   │   WorkflowExecution                 │
│   Protocols/      │                                     │
│   ProtocolFactory │                                     │
│   RdpProtocol     │                                     │
│   SshProtocol     │                                     │
│   VncProtocol     │                                     │
└───────────────────┴─────────────────────────────────────┘
```

### Design Patterns

| Pattern | Usage |
|---|---|
| **MVVM** | All UI code; ViewModels expose `ObservableProperty` and `RelayCommand` |
| **Dependency Injection** | All services and ViewModels; registered in `App.xaml.cs` |
| **Repository (via EF Core)** | `ApplicationDbContext` for all data access |
| **Service Layer** | All business logic behind `IXxxService` interfaces |
| **Command Pattern** | `RelayCommand` / `AsyncRelayCommand` for all UI actions |
| **Factory Pattern** | `IProtocolFactory` / `ProtocolFactory` for creating protocol instances |
| **Observer** | Data binding via `INotifyPropertyChanged` and `ObservableCollection<T>` |

### Dependency Injection Registration (`App.xaml.cs`)

Services are registered with these lifetimes:

| Lifetime | Used For |
|---|---|
| `AddDbContext` (Scoped) | `ApplicationDbContext` |
| `Scoped` | All `IXxxService` implementations |
| `Transient` | All ViewModels |
| `Singleton` | Theme service, notification service |

---

## 5. Feature Reference

### 5.1 Connection Profile Management

A **Connection Profile** stores all settings needed to connect to a remote machine.

#### Profile Fields

| Field | Type | Description |
|---|---|---|
| `Name` | string (max 200) | Friendly name, e.g. "Production Server" |
| `ServerAddress` | string (max 255) | IP address or hostname |
| `Port` | int (default 3389) | Port number |
| `Username` | string? (max 100) | Windows username |
| `Domain` | string? (max 100) | Windows domain |
| `IsFavorite` | bool | Starred for quick access |
| `GroupName` | string? (max 100) | Logical group |
| `Tags` | JSON array string | Arbitrary labels |
| `Settings` | JSON object string | Per-profile display settings |
| `ProtocolType` | enum | `RDP` (0), `SSH` (1), `VNC` (2) |
| `ProtocolSpecificSettings` | JSON? | Extra settings per protocol |
| `MacAddress` | string? | For Wake-on-LAN |
| `EnableWakeOnLan` | bool | Activate WoL for this profile |
| `WakeTimeoutSeconds` | int (default 30) | Seconds to wait after WoL |
| `UseSshTunnel` | bool | Route through SSH tunnel |
| `SshTunnelConfig` | JSON? | SSH tunnel configuration |
| `CreatedAt` | DateTime | Profile creation timestamp |
| `LastConnectedAt` | DateTime? | Last successful connection |
| `ConnectionCount` | int | Total successful connections |

#### Operations

- **Create** — Fill in details in the "New Connection" dialog; click Save
- **Edit** — Select a profile and click Edit; modify fields; click Save
- **Delete** — Select a profile and click Delete; confirm the deletion
- **Favourite** — Click the ⭐ icon to toggle favourite status
- **Search** — Type in the search box; matches Name, Server, Username, Group, Tags in real time
- **Filter by Group** — Use the Group dropdown to show only connections in a group
- **Filter by Tag** — Use the Tag dropdown to filter by tag

#### Import / Export

See [Section 5.12](#512-import--export).

---

### 5.2 Multi-Protocol Support (RDP / SSH / VNC)

Aivana supports three remote access protocols through a unified abstraction layer.

#### Protocol Types

| Protocol | Enum Value | Default Port | Description |
|---|---|---|---|
| RDP | 0 | 3389 | Windows Remote Desktop via MSTSC ActiveX |
| SSH | 1 | 22 | Secure Shell terminal access |
| VNC | 2 | 5900 | Virtual Network Computing |

#### Protocol Abstraction

Each protocol implements `IRemoteProtocol`:

```csharp
public interface IRemoteProtocol
{
    ProtocolType Type { get; }
    ProtocolCapabilities Capabilities { get; }
    Task ConnectAsync(ConnectionProfile profile, string? password);
    Task DisconnectAsync();
    bool IsConnected { get; }
}
```

The `ProtocolFactory` creates the correct implementation based on `ConnectionProfile.ProtocolType`.

#### RDP Implementation

Uses the Windows **MSTSC ActiveX Control** (`mstscax.dll`) wrapped in `RdpClientWrapper`:
- Hosted in a `WindowsFormsHost` embedded in WPF
- Supports full-screen toggle, scaling refresh, and per-profile configuration
- Connection state tracked via `RdpConnectionService`

---

### 5.3 Workspace & Session Management

The workspace provides a tabbed interface for managing multiple simultaneous remote sessions and tools.

#### Tab Types

| TabType | Description |
|---|---|
| `RemoteSession` | Active RDP/SSH/VNC session |
| `LocalTerminal` | Local command-line terminal |
| `FileExplorer` | File explorer panel |
| `Settings` | Application settings |
| `Dashboard` | Connection overview & metrics |
| `Custom` | Extension or custom content |

#### Tab Properties

Each `WorkspaceTab` has:
- `Title` and `Icon` displayed in the tab header
- `Status`: Loading, Connected, Disconnected, Error, Ready
- `IsClosable` — can be pinned by setting to `false`
- `CreatedAt` and `LastActivatedAt` timestamps
- `Metadata` dictionary for extension data

#### Session State

Each `RemoteSession` tracks:

| Property | Description |
|---|---|
| `Status` | Connecting, Connected, Disconnected, Suspended, Error, Reconnecting |
| `Duration` | Computed from `ConnectedAt` → `DisconnectedAt` |
| `CurrentMetrics` | Live `PerformanceMetrics` snapshot |
| `State.IsFullScreen` | Full-screen mode flag |
| `State.ZoomLevel` | Zoom factor (default 1.0) |
| `State.AudioRedirected` | Audio redirection active |
| `State.PrinterRedirected` | Printer redirection active |
| `State.ClipboardRedirected` | Clipboard redirection active |
| `Events` | List of `SessionEvent` items (audit log) |

#### Session Events

Events tracked per session:

| EventType | Trigger |
|---|---|
| `Connected` | Successful connection established |
| `Disconnected` | Session ended |
| `Error` | Connection error occurred |
| `Warning` | Non-fatal issue |
| `FileTransferStarted` | File transfer initiated |
| `FileTransferCompleted` | File transfer finished |
| `ClipboardUpdated` | Clipboard content changed |
| `SettingsChanged` | Session settings modified |

#### Layout Persistence

- `WorkspaceLayout` captures the set of open tabs and their arrangement
- `ILayoutPersistenceService` saves/restores layouts to disk
- Layouts are restored on application startup

---

### 5.4 Wake-on-LAN

Wake-on-LAN (WoL) allows you to power on a remote machine before connecting.

#### Prerequisites

- The remote machine must have WoL enabled in its BIOS/UEFI settings
- The network must support directed broadcast or subnet broadcast packets
- The MAC address of the remote machine must be known

#### Configuration

In the connection profile:
- **Enable Wake-on-LAN**: `true`
- **MAC Address**: e.g. `AA:BB:CC:DD:EE:FF`
- **Wake Timeout (seconds)**: How long to wait for the machine to respond (default: 30)

#### How It Works

1. `IWakeOnLanService.WakeConnectionAsync(profile)` is called
2. Validates the MAC address format
3. Sends a UDP magic packet to the configured broadcast address
4. Polls the host address until it responds or the timeout expires
5. Returns a `WakeResult` indicating success or failure

#### API

```csharp
public interface IWakeOnLanService
{
    Task<WakeResult> SendWakePacketAsync(string macAddress, string broadcastAddress, CancellationToken ct = default);
    Task<bool> WaitForHostAsync(string hostAddress, TimeSpan timeout, CancellationToken ct = default);
    Task<WakeResult> WakeConnectionAsync(ConnectionProfile profile, CancellationToken ct = default);
    Task<bool> ValidateMacAddressAsync(string macAddress);
}
```

---

### 5.5 SSH Tunnelling

SSH tunnels allow you to route RDP or VNC connections securely through an SSH jump host.

#### Configuration (`SshTunnelConfiguration`)

| Field | Description |
|---|---|
| SSH host address | Hostname or IP of the SSH jump host |
| SSH port | Default: 22 |
| SSH username | Username on the jump host |
| SSH private key path | Path to private key file (or use password) |
| Local forwarding port | Port on local machine to forward |
| Remote target host | The actual remote machine address |
| Remote target port | Port on the remote target |

#### How It Works

1. When connecting to a profile with `UseSshTunnel = true`, the SSH tunnel is created first
2. `ISshTunnelService.CreateTunnelAsync(config)` establishes the tunnel
3. The remote session connects through the tunnel's local forwarding port
4. On disconnect, `CloseTunnelAsync(tunnelId)` tears down the tunnel

#### API

```csharp
public interface ISshTunnelService
{
    Task<TunnelResult> CreateTunnelAsync(SshTunnelConfiguration config, CancellationToken ct = default);
    Task CloseTunnelAsync(string tunnelId, CancellationToken ct = default);
    Task<bool> IsTunnelActiveAsync(string tunnelId, CancellationToken ct = default);
    Task<List<TunnelInfo>> GetActiveTunnelsAsync(CancellationToken ct = default);
}
```

---

### 5.6 Workflow Engine

The workflow engine allows scriptless automation of complex remote access tasks.

#### Core Concepts

| Concept | Description |
|---|---|
| `WorkflowDefinition` | Named workflow with ordered steps and a trigger |
| `WorkflowStep` | Individual action unit with parameters and conditions |
| `WorkflowTrigger` | What starts the workflow (manual, schedule, event) |
| `WorkflowExecution` | Runtime record of a workflow run |

#### Workflow Status

`Draft` → `Active` → (`Paused` / `Disabled` / `Error`)

#### Trigger Types

| Trigger | Description |
|---|---|
| `Manual` | User-initiated from UI |
| `Scheduled` | Time-based (cron-like schedule) |
| `OnConnection` | Fires when a connection is established |
| `OnDisconnection` | Fires when a connection ends |
| `OnFileChange` | Fires when a file system change is detected |
| `OnSystemEvent` | Fires on a Windows system event |
| `OnHotkey` | Fires when a configured key combination is pressed |
| `OnApplicationStart` | Fires when the application starts |

#### Step Types

| Step Type | Description |
|---|---|
| `ConnectRemote` | Connect to a named connection profile |
| `DisconnectRemote` | Disconnect from a named session |
| `StartApplication` | Launch a local or remote application |
| `StopApplication` | Terminate an application |
| `TransferFile` | Upload or download a file |
| `ExecuteCommand` | Run a shell command locally or remotely |
| `SendKeystrokes` | Send keyboard input to the active session |
| `TakeScreenshot` | Capture the current session screen |
| `WaitCondition` | Pause until a condition is met |
| `ShowNotification` | Display a Windows notification |
| `LogMessage` | Write a message to the workflow log |
| `SetVariable` | Set or update a workflow variable |
| `IfCondition` | Branch workflow based on a condition |
| `LoopSteps` | Repeat a set of steps |
| `ParallelSteps` | Execute steps concurrently |
| `Delay` | Wait for a specified duration |
| `CustomScript` | Execute a custom script |

#### Step Conditions

Each step can have a `WorkflowStepCondition`:

```
VariableName  Operator  ExpectedValue
─────────────────────────────────────
status        Equals    "Connected"
latency       LessThan  100
```

Supported operators: `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `Contains`, `StartsWith`, `EndsWith`, `IsNull`, `IsNotNull`

#### Variables

Workflows support a `Variables` dictionary of `string → object` entries that persist across steps and can be set with `SetVariable` and read in conditions or parameters.

#### Example Workflow

```
Name: "Wake and Connect Production Server"
Trigger: Manual

Steps:
  1. WakeOnLan      → target = "Production Server" profile
  2. WaitCondition  → condition: host is reachable
  3. Delay          → 5 seconds
  4. ConnectRemote  → profile = "Production Server"
  5. ShowNotification → "Connected to Production Server"
```

---

### 5.7 File Transfer

File transfer enables moving files between your local machine and a remote desktop session.

#### Operations

| Operation | Description |
|---|---|
| Upload | Transfer file from local machine to remote session |
| Download | Transfer file from remote session to local machine |
| Pause | Temporarily halt an in-progress transfer |
| Resume | Continue a paused transfer |
| Cancel | Abort a transfer |

#### Transfer State Tracking (`FileTransferInfo`)

| Field | Description |
|---|---|
| `FileName` | Name of the transferred file |
| `FileSize` | Total size in bytes |
| `BytesTransferred` | Current progress in bytes |
| `Progress` | Percentage complete (0–100) |
| `Status` | Pending / InProgress / Completed / Failed / Cancelled |
| `Speed` | Current transfer speed (bytes/s) |
| `StartedAt` | Transfer start timestamp |
| `CompletedAt` | Transfer completion timestamp |
| `ErrorMessage` | Error description on failure |

> **Note:** File transfer v1.0 uses simulation mode. Full RDP Virtual Channels integration is planned for v1.1.

---

### 5.8 Clipboard Synchronisation

Clipboard sync provides bidirectional content sharing between local machine and remote sessions.

#### Supported Formats

- **Text** — plain text strings
- **Images** — bitmap clipboard entries
- **File Lists** — file path collections (planned)

#### Clipboard History

- Last 50 clipboard items are tracked per session (`ClipboardItem` model)
- Users can browse history and re-paste previous entries

#### Configuration

- Enabled by default for all connections
- Can be disabled per connection in profile settings
- `IClipboardService` provides full async API for getting, setting, and listing items

> **Note:** Full binary format preservation (Rich Text, HTML, custom formats) is in development for v1.1.

---

### 5.9 Performance Monitoring

Performance monitoring provides real-time visibility into connection health.

#### Metrics Collected

| Metric | Unit | Description |
|---|---|---|
| `LatencyMs` | ms | Round-trip network latency |
| `BandwidthMbps` | Mbps | Available network bandwidth |
| `PacketLossPercent` | % | Network packet loss rate |
| `FrameRate` | FPS | Remote desktop rendering frame rate |
| `QualityScore` | 0–100 | Overall connection quality index |
| `CpuUsagePercent` | % | Local CPU utilisation |
| `MemoryUsageMB` | MB | Local memory usage |
| `NetworkUsageMbps` | Mbps | Local network interface usage |

#### Quality Levels

| Score Range | Quality Level |
|---|---|
| ≥ 90 | Excellent |
| 75–89 | Good |
| 60–74 | Fair |
| 40–59 | Poor |
| < 40 | VeryPoor |

#### Data Persistence

`PerformanceMetrics` records are stored in the SQLite database linked to the `ConnectionProfile`, enabling historical trend analysis.

> **Note:** v1.0 collects local metrics. Remote metrics (remote CPU/memory) via RDP virtual channels are planned for v1.1.

---

### 5.10 Session Recording

Session recording captures remote desktop activity to disk.

#### Controls

| Action | Description |
|---|---|
| Start | Begin recording the current session |
| Pause | Temporarily suspend recording |
| Resume | Continue a paused recording |
| Stop | Finalise and save the recording |

#### `SessionRecording` Model

| Field | Description |
|---|---|
| `ConnectionProfileId` | Linked connection profile |
| `StartedAt` | Recording start time |
| `EndedAt` | Recording end time |
| `FilePath` | Path to the recording file on disk |
| `FileSize` | File size in bytes |
| `Duration` | Total recording duration |
| `Status` | Recording / Paused / Completed / Failed |
| `Notes` | User-provided notes |

> **Note:** v1.0 creates placeholder files. Full video recording via Windows Media Foundation is planned for v1.1.

---

### 5.11 Security & Credential Management

#### Credential Storage

Credentials are stored in the **Windows Credential Manager** via the native API:
- Passwords are encrypted by Windows using the user's Windows account
- No credentials are stored in plain text in the database or log files
- `ICredentialService` / `WindowsCredentialManager` handle all credential operations

#### TLS / Encryption

- All RDP connections use TLS 1.2 or higher when the server supports it
- Invalid or self-signed certificates trigger a user-visible warning

#### Audit Logging

- All connection events are logged (connect, disconnect, errors)
- Timestamps and connection profile identifiers are included
- Log files are written to `%AppData%\Aivana_RDP_WPF\Logs\`

#### Session Security

- Session data is never written to plain-text log entries
- Credentials are not included in any log output

---

### 5.12 Import / Export

#### Supported Formats

| Format | Import | Export | Notes |
|---|---|---|---|
| `.rdp` | ✅ | ✅ | Standard Windows RDP file |
| JSON | ✅ | ✅ | Full profile fidelity |
| CSV | ✅ | ✅ | Tabular; compatible with Excel |

#### Usage

- **Import**: Toolbar → Import → choose file → select profiles to import
- **Export**: Toolbar → Export → choose format → save file

`IImportExportService` handles parsing, mapping, and validation for all formats.

---

### 5.13 Themes & UI Customisation

#### Available Themes

| Theme | Description |
|---|---|
| Light | Clean white/grey Fluent Design |
| Dark | Dark background optimised for low-light |
| System | Follows Windows system theme preference |

#### Switching Themes

1. Open **Settings**
2. Select **Theme**
3. Choose **Light**, **Dark**, or **System**
4. Theme applies instantly — no restart required

#### Fluent Design Implementation

- `FluentDesignStyles.xaml` — core style definitions
- `LightTheme.xaml` / `DarkTheme.xaml` — theme-specific colour and brush overrides
- `IThemeService` manages runtime theme switching via resource dictionary replacement

---

## 6. Configuration Reference

Configuration is loaded from `appsettings.json` (and overridden by `appsettings.Development.json` in development mode). Environment variables are also supported.

### Default Configuration (`appsettings.json`)

```json
{
  "ApplicationSettings": {
    "DefaultConnection": {
      "Port": 3389,
      "ColorDepth": 32,
      "Resolution": "1920x1080"
    },
    "UI": {
      "Theme": "System",
      "Language": "en-US"
    },
    "Updates": {
      "AutoUpdate": false,
      "CheckFrequency": "Weekly"
    },
    "Performance": {
      "MaxConcurrentConnections": 10,
      "ConnectionTimeout": 30000
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Configuration Keys

| Key | Type | Default | Description |
|---|---|---|---|
| `ApplicationSettings.DefaultConnection.Port` | int | `3389` | Default RDP port for new profiles |
| `ApplicationSettings.DefaultConnection.ColorDepth` | int | `32` | Colour depth in bits (16, 24, 32) |
| `ApplicationSettings.DefaultConnection.Resolution` | string | `"1920x1080"` | Default display resolution |
| `ApplicationSettings.UI.Theme` | string | `"System"` | `"Light"`, `"Dark"`, `"System"` |
| `ApplicationSettings.UI.Language` | string | `"en-US"` | UI locale |
| `ApplicationSettings.Updates.AutoUpdate` | bool | `false` | Enable automatic updates |
| `ApplicationSettings.Updates.CheckFrequency` | string | `"Weekly"` | `"Daily"`, `"Weekly"`, `"Monthly"` |
| `ApplicationSettings.Performance.MaxConcurrentConnections` | int | `10` | Maximum simultaneous sessions |
| `ApplicationSettings.Performance.ConnectionTimeout` | int | `30000` | Connection timeout in milliseconds |
| `Logging.LogLevel.Default` | string | `"Information"` | Minimum log level |
| `Logging.LogLevel.Microsoft` | string | `"Warning"` | MS framework log level |

### Development Override (`appsettings.Development.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

---

## 7. Data Models

### ConnectionProfile

Core entity persisted to SQLite. See [Section 5.1](#51-connection-profile-management) for field details.

**Relations:**
- `SessionHistories` → `ICollection<SessionHistory>` (one-to-many)
- `PerformanceMetrics` → `ICollection<PerformanceMetrics>` (one-to-many, via FK)

### SessionHistory

Records each connection session.

| Field | Type | Description |
|---|---|---|
| `Id` | int (PK) | Auto-increment |
| `ConnectionProfileId` | int (FK) | Linked profile |
| `ConnectedAt` | DateTime | Session start |
| `DisconnectedAt` | DateTime? | Session end |
| `Duration` | TimeSpan? | Session length |
| `EndReason` | string | Disconnect reason |

### PerformanceMetrics

Timestamped metric snapshot per connection profile. See [Section 5.9](#59-performance-monitoring) for fields.

### WorkflowDefinition

Automation workflow. See [Section 5.6](#56-workflow-engine) for full model details.

### WorkflowExecution

Runtime record of a workflow run.

| Field | Type | Description |
|---|---|---|
| `Id` | string (GUID) | Execution ID |
| `WorkflowDefinitionId` | string | Linked workflow |
| `StartedAt` | DateTime | Execution start |
| `CompletedAt` | DateTime? | Execution end |
| `Status` | enum | Running / Completed / Failed / Cancelled |
| `StepResults` | List | Per-step `StepExecutionResult` |
| `Variables` | Dictionary | Runtime variable state |
| `ErrorMessage` | string? | Error on failure |

### WorkspaceTab

See [Section 5.3](#53-workspace--session-management) for field details.

### WorkspaceLayout

| Field | Type | Description |
|---|---|---|
| `Id` | string (GUID) | Layout ID |
| `Name` | string | Layout name |
| `Tabs` | List<WorkspaceTab> | Captured tab set |
| `ActiveTabId` | string? | Currently active tab |
| `SavedAt` | DateTime | Snapshot timestamp |

### ClipboardItem

| Field | Type | Description |
|---|---|---|
| `Id` | string (GUID) | Item ID |
| `Content` | string | Clipboard content |
| `Format` | string | Clipboard format (Text, Image, etc.) |
| `CapturedAt` | DateTime | When captured |
| `Source` | string | Local or remote |

### FileTransferInfo

See [Section 5.7](#57-file-transfer) for field details.

### SessionRecording

See [Section 5.10](#510-session-recording) for field details.

### SshTunnelConfiguration

| Field | Type | Description |
|---|---|---|
| `SshHost` | string | Jump host address |
| `SshPort` | int | SSH port (default 22) |
| `SshUsername` | string | Username on jump host |
| `PrivateKeyPath` | string? | Path to SSH private key |
| `LocalPort` | int | Local forwarding port |
| `RemoteHost` | string | Target host |
| `RemotePort` | int | Target port |

### ApplicationSettings

| Field | Type | Description |
|---|---|---|
| `Theme` | string | Current theme |
| `Language` | string | UI language |
| `MaxConcurrentConnections` | int | Session limit |
| `DefaultPort` | int | Default RDP port |
| `DefaultColorDepth` | int | Default colour depth |
| `DefaultResolution` | string | Default resolution |
| `AutoUpdate` | bool | Auto-update enabled |

---

## 8. Service Layer API

All services follow interface-first design. Use the interface for dependency injection, never the concrete class directly.

### IConnectionProfileService

```csharp
Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default);
Task<ConnectionProfile?> GetProfileByIdAsync(int id, CancellationToken ct = default);
Task<ConnectionProfile> CreateProfileAsync(ConnectionProfile profile, CancellationToken ct = default);
Task<ConnectionProfile> UpdateProfileAsync(ConnectionProfile profile, CancellationToken ct = default);
Task DeleteProfileAsync(int id, CancellationToken ct = default);
Task<List<ConnectionProfile>> SearchProfilesAsync(string query, CancellationToken ct = default);
Task<List<ConnectionProfile>> GetFavoritesAsync(CancellationToken ct = default);
Task<List<string>> GetGroupsAsync(CancellationToken ct = default);
Task UpdateLastConnectedAsync(int profileId, CancellationToken ct = default);
```

### IRdpConnectionService

```csharp
WindowsFormsHost? CreateConnectionHost(ConnectionProfile profile);
Task ConnectAsync(ConnectionProfile profile, string? password = null);
void Connect(ConnectionProfile profile, string? password = null);
void Disconnect(int profileId);
bool IsConnected(int profileId);
void SetFullScreen(int profileId, bool fullScreen);
void RefreshScaling(int profileId);
RdpClientWrapper? GetActiveWrapper(int profileId);
```

### IFileTransferService

```csharp
Task<FileTransferInfo> StartUploadAsync(string localPath, string remotePath, ConnectionProfile profile, CancellationToken ct = default);
Task<FileTransferInfo> StartDownloadAsync(string remotePath, string localPath, ConnectionProfile profile, CancellationToken ct = default);
Task PauseTransferAsync(string transferId);
Task ResumeTransferAsync(string transferId);
Task CancelTransferAsync(string transferId);
Task<List<FileTransferInfo>> GetTransferHistoryAsync();
```

### IClipboardService

```csharp
Task<string?> GetClipboardContentAsync();
Task SetClipboardContentAsync(string content);
Task<List<ClipboardItem>> GetClipboardHistoryAsync(int profileId);
Task SyncClipboardAsync(int profileId, CancellationToken ct = default);
Task ClearHistoryAsync(int profileId);
```

### IPerformanceMonitorService

```csharp
Task<PerformanceMetrics> GetCurrentMetricsAsync(int profileId, CancellationToken ct = default);
Task<List<PerformanceMetrics>> GetMetricsHistoryAsync(int profileId, DateTime from, DateTime to, CancellationToken ct = default);
Task StartMonitoringAsync(int profileId, CancellationToken ct = default);
Task StopMonitoringAsync(int profileId);
```

### ISessionRecordingService

```csharp
Task<SessionRecording> StartRecordingAsync(int profileId, string outputPath, CancellationToken ct = default);
Task PauseRecordingAsync(string recordingId);
Task ResumeRecordingAsync(string recordingId);
Task<SessionRecording> StopRecordingAsync(string recordingId);
Task<List<SessionRecording>> GetRecordingHistoryAsync(int profileId);
Task DeleteRecordingAsync(string recordingId);
```

### IWakeOnLanService

```csharp
Task<WakeResult> SendWakePacketAsync(string macAddress, string broadcastAddress, CancellationToken ct = default);
Task<bool> WaitForHostAsync(string hostAddress, TimeSpan timeout, CancellationToken ct = default);
Task<WakeResult> WakeConnectionAsync(ConnectionProfile profile, CancellationToken ct = default);
Task<bool> ValidateMacAddressAsync(string macAddress);
```

### ISshTunnelService

```csharp
Task<TunnelResult> CreateTunnelAsync(SshTunnelConfiguration config, CancellationToken ct = default);
Task CloseTunnelAsync(string tunnelId, CancellationToken ct = default);
Task<bool> IsTunnelActiveAsync(string tunnelId, CancellationToken ct = default);
Task<List<TunnelInfo>> GetActiveTunnelsAsync(CancellationToken ct = default);
```

### IWorkflowEngineService

```csharp
Task<WorkflowDefinition> CreateWorkflowAsync(WorkflowDefinition definition, CancellationToken ct = default);
Task<WorkflowExecution> ExecuteWorkflowAsync(string workflowId, CancellationToken ct = default);
Task<WorkflowExecution> GetExecutionStatusAsync(string executionId, CancellationToken ct = default);
Task PauseExecutionAsync(string executionId);
Task CancelExecutionAsync(string executionId);
Task<List<WorkflowDefinition>> GetWorkflowsAsync(CancellationToken ct = default);
Task DeleteWorkflowAsync(string workflowId);
```

### ISessionManagerService

```csharp
Task<RemoteSession> CreateSessionAsync(ConnectionProfile profile, CancellationToken ct = default);
Task<RemoteSession?> GetSessionAsync(string sessionId);
Task<List<RemoteSession>> GetActiveSessionsAsync();
Task CloseSessionAsync(string sessionId);
Task<SessionHistory> GetSessionHistoryAsync(int profileId, CancellationToken ct = default);
```

### IWorkspaceService

```csharp
Task<WorkspaceTab> AddTabAsync(TabType type, string title, object? content = null);
Task CloseTabAsync(string tabId);
Task<WorkspaceTab?> GetActiveTabAsync();
Task SetActiveTabAsync(string tabId);
Task<WorkspaceLayout> SaveLayoutAsync(string name);
Task<WorkspaceLayout?> RestoreLayoutAsync(string layoutId);
Task<List<WorkspaceLayout>> GetSavedLayoutsAsync();
```

### IImportExportService

```csharp
Task<List<ConnectionProfile>> ImportFromRdpFileAsync(string filePath, CancellationToken ct = default);
Task<List<ConnectionProfile>> ImportFromJsonAsync(string filePath, CancellationToken ct = default);
Task<List<ConnectionProfile>> ImportFromCsvAsync(string filePath, CancellationToken ct = default);
Task ExportToJsonAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default);
Task ExportToCsvAsync(IEnumerable<ConnectionProfile> profiles, string filePath, CancellationToken ct = default);
Task ExportToRdpAsync(ConnectionProfile profile, string filePath, CancellationToken ct = default);
```

### ICredentialService

```csharp
Task SaveCredentialAsync(string target, string username, string password);
Task<(string username, string password)?> GetCredentialAsync(string target);
Task DeleteCredentialAsync(string target);
```

### IThemeService

```csharp
void ApplyTheme(string theme);    // "Light" | "Dark" | "System"
string GetCurrentTheme();
event EventHandler<string> ThemeChanged;
```

### INotificationService

```csharp
void ShowInfo(string title, string message);
void ShowWarning(string title, string message);
void ShowError(string title, string message);
void ShowSuccess(string title, string message);
```

---

## 9. Keyboard Shortcuts

### General

| Shortcut | Action |
|---|---|
| `Ctrl+N` | New Connection |
| `Ctrl+F` | Focus Search Box |
| `Ctrl+R` | Refresh Connection List |
| `F5` | Refresh |
| `Ctrl+,` | Open Settings |
| `F1` | Help |

### Connection Management

| Shortcut | Action |
|---|---|
| `Enter` | Connect to selected connection |
| `Delete` | Delete selected connection |
| `F2` | Edit selected connection |
| `Ctrl+E` | Export connections |
| `Ctrl+I` | Import connections |

### Session / Workspace

| Shortcut | Action |
|---|---|
| `Ctrl+T` | New Session Tab |
| `Ctrl+W` | Close Current Tab |
| `Ctrl+Tab` | Switch to Next Tab |
| `Ctrl+Shift+Tab` | Switch to Previous Tab |
| `Ctrl+1` – `Ctrl+9` | Switch to Tab by Number |

### Within an RDP Session

| Shortcut | Action |
|---|---|
| `Ctrl+Alt+End` | Send Ctrl+Alt+Del to Remote |
| `F11` | Toggle Full Screen |
| `Alt+Enter` | Toggle Full Screen |

---

## 10. Logging & Diagnostics

### Log Locations

| Log Type | Location |
|---|---|
| File logs | `%AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log` |
| Console logs | Visual Studio Output window (Debug builds) |

### Log Levels

| Level | Description |
|---|---|
| `Trace` | Very detailed; rarely needed |
| `Debug` | Diagnostic information for development |
| `Information` | General operational messages (default) |
| `Warning` | Non-fatal issues |
| `Error` | Failures that affect functionality |
| `Critical` | Severe failures requiring immediate attention |

### Enabling Debug Logging

Edit `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Aivana_RDP_WPF": "Trace"
    }
  }
}
```

### Log Rotation

Log files rotate daily. Old files are retained for 30 days then deleted automatically.

---

## 11. Database

### Location

`%AppData%\Aivana_RDP_WPF\aivana.db` (SQLite)

### Tables

| Table | Model | Description |
|---|---|---|
| `ConnectionProfiles` | `ConnectionProfile` | Saved connection profiles |
| `SessionHistories` | `SessionHistory` | Per-session history records |
| `PerformanceMetrics` | `PerformanceMetrics` | Historical metric snapshots |
| `ApplicationSettings` | `AppSetting` | Key-value settings |

### Migrations

Migrations are applied automatically on application startup.

To manage migrations manually:

```bash
# Add a new migration
cd Aivana_RDP_WPF
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Reset database (WARNING: deletes all data)
rm %AppData%\Aivana_RDP_WPF\aivana.db
dotnet ef database update
```

### Backup

Copy `%AppData%\Aivana_RDP_WPF\aivana.db` to a safe location. The file can be restored by placing it back in the same directory.

---

## 12. Developer Guide

### Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 8.0+ | Build and run |
| Visual Studio 2022 | 17.8+ | IDE (`.NET desktop development` workload) |
| Windows SDK | 10.0.22000+ | Windows-specific APIs |
| Git | Any | Version control |
| EF Core Tools | 8.0+ | Database migrations |

Install EF Core tools:

```bash
dotnet tool install --global dotnet-ef
```

### Building

```bash
# Debug build
dotnet build Aivana_RDP_WPF.sln

# Release build
dotnet build Aivana_RDP_WPF.sln -c Release

# Clean
dotnet clean
```

Build output:
- **Debug**: `Aivana_RDP_WPF/bin/Debug/net8.0-windows/`
- **Release**: `Aivana_RDP_WPF/bin/Release/net8.0-windows/`

### Adding a New Service

1. **Define the interface** in `Services/IMyService.cs`
2. **Implement it** in `Services/MyService.cs`
3. **Register** in `App.xaml.cs → ConfigureServices`:
   ```csharp
   services.AddScoped<IMyService, MyService>();
   ```

### Adding a New ViewModel

```csharp
// ViewModels/MyFeatureViewModel.cs
public partial class MyFeatureViewModel : ObservableObject
{
    private readonly IMyService _service;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MyFeatureViewModel(IMyService service)
    {
        _service = service;
    }

    [RelayCommand]
    private async Task ExecuteAsync(CancellationToken ct)
    {
        StatusMessage = "Working...";
        await _service.DoWorkAsync(ct);
        StatusMessage = "Done";
    }
}
```

Register: `services.AddTransient<MyFeatureViewModel>();`

### Adding a New View

```xml
<!-- Views/MyFeatureView.xaml -->
<UserControl x:Class="Aivana_RDP_WPF.Views.MyFeatureView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel>
        <Button Content="Execute" Command="{Binding ExecuteCommand}" />
        <TextBlock Text="{Binding StatusMessage}" />
    </StackPanel>
</UserControl>
```

### Adding a Protocol

1. Implement `IRemoteProtocol` in `Infrastructure/Protocols/MyProtocol.cs`
2. Add a value to the `ProtocolType` enum
3. Register the implementation in `ProtocolFactory`

### Code Style

| Convention | Rule |
|---|---|
| Classes, methods, properties | PascalCase |
| Local variables, parameters | camelCase |
| Private fields | `_camelCase` |
| Constants | `UPPER_CASE` |
| Async methods | Suffix `Async` |
| I/O operations | Always `async/await` |
| Public APIs | XML `<summary>` documentation |
| Null handling | Nullable reference types enabled; always check |

### Commit Message Format

```
<type>: <short description>

Types: feat | fix | docs | refactor | test | chore
```

Examples:
- `feat: add Wake-on-LAN status indicator in connection list`
- `fix: resolve crash when connecting with empty username`
- `docs: update SSH tunnel configuration guide`

---

## 13. Testing

### Test Project

`Aivana_RDP_WPF.Tests/`

### Running Tests

```bash
# All tests
dotnet test Aivana_RDP_WPF.Tests/Aivana_RDP_WPF.Tests.csproj

# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only
dotnet test --filter Category=Integration

# With code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Categories

| Category | Description |
|---|---|
| Unit | Service logic tests with mocked dependencies |
| Integration | Tests using real SQLite database |
| System | End-to-end UI and connection tests |
| Performance | Benchmarks for key operations |
| Security | Credential and encryption tests |

### Testing Services

Use mock implementations of `IXxxService` interfaces (e.g. with Moq):

```csharp
var mockService = new Mock<IConnectionProfileService>();
mockService.Setup(s => s.GetAllProfilesAsync(default))
           .ReturnsAsync(new List<ConnectionProfile>());

var vm = new ConnectionListViewModel(mockService.Object, ...);
```

---

## 14. Troubleshooting

### Cannot Connect to Remote Desktop

| Check | Action |
|---|---|
| Server address / port | Verify in profile settings |
| RDP enabled on remote | Run `sysdm.cpl → Remote` on remote machine |
| Firewall | Allow port 3389 (TCP) on remote machine |
| Network connectivity | `ping <serverAddress>` from local machine |
| Credentials | Re-enter credentials in profile settings |

### Connection Timeout

1. Verify the remote machine is powered on (use Wake-on-LAN if needed)
2. Check `ApplicationSettings.Performance.ConnectionTimeout` in `appsettings.json`
3. Verify no VPN or proxy is interfering

### Credential Manager Error

1. Run the application as Administrator (if required by your environment)
2. Open Windows Credential Manager and manually verify the credential entry
3. Delete the entry in Windows Credential Manager and re-enter in the application

### Slow Connection / High Latency

1. Check network bandwidth (use speed test)
2. Reduce colour depth to 16-bit in the connection profile settings
3. Enable compression in advanced connection settings
4. Close unused session tabs and background applications
5. Check the Performance Monitoring dashboard for bottleneck indicators

### Application Won't Start

1. Verify .NET 8.0 Runtime is installed: `dotnet --version`
2. Check Windows Event Viewer (Application log) for error entries
3. Delete `%AppData%\Aivana_RDP_WPF\aivana.db` to reset database (loses saved connections)
4. Reinstall / rebuild the application

### Database Errors

1. Check permissions on `%AppData%\Aivana_RDP_WPF\`
2. Check disk space
3. Delete the `.db` file to allow automatic recreation on next start (data lost)

### RDP ActiveX Control Not Found

Symptom: `COM class not registered` or `mstscax.dll` errors.

1. Verify Windows Remote Desktop Connection is installed (run `mstsc.exe`)
2. Register the DLL: `regsvr32 C:\Windows\System32\mstscax.dll` (as Administrator)

### Log File Locations

```
%AppData%\Aivana_RDP_WPF\Logs\aivana-YYYY-MM-DD.log
%AppData%\Aivana_RDP_WPF\aivana.db
```

---

## 15. Contributing

### Quick Start

1. Fork the repository on GitHub
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make changes following the [code style guide](#code-style)
4. Add unit tests for new logic
5. Run tests: `dotnet test`
6. Commit: `git commit -m "feat: describe your change"`
7. Push: `git push origin feature/my-feature`
8. Open a Pull Request on GitHub

### Pull Request Guidelines

- Keep PRs focused — one feature or fix per PR
- Include test coverage for new functionality
- Update documentation if you add or change behaviour
- Reference any related GitHub Issues in the PR description

### Reporting Bugs

Use [GitHub Issues](https://github.com/rweisssieker-xp/AIVANA-RDP-WPF/issues) with:
- Steps to reproduce
- Expected behaviour
- Actual behaviour
- Environment (OS version, .NET version, app version)
- Log file excerpts if relevant

---

## 16. Changelog Summary

### v1.0.0 (2025-11-27) — Initial Release

**Core Features:**
- Multi-protocol connectivity: RDP (MSTSC ActiveX), SSH, VNC
- Full connection profile CRUD with groups, tags, favourites, search
- Import/export: `.rdp`, JSON, CSV
- Wake-on-LAN with host availability polling
- SSH tunnelling through jump hosts
- Workflow engine with 17 step types and 8 trigger types
- Workspace tab management with layout persistence
- Quick Connect (connect without saving a profile)
- File transfer (simulation mode — full VirtualChannels in v1.1)
- Clipboard synchronisation (basic text and image)
- Performance monitoring (local metrics — remote in v1.1)
- Session recording (placeholder files — video encoding in v1.1)
- Fluent Design 2.0 UI with Light/Dark/System themes
- SQLite persistence via Entity Framework Core
- Structured logging with daily file rotation
- Full MVVM architecture with Dependency Injection

**Known Limitations:**
- File transfer uses simulation; real transfers pending RDP Virtual Channels integration
- Clipboard: full binary format preservation in development
- Performance monitoring: remote CPU/memory metrics pending
- Session recording: full video encoding pending Windows Media Foundation integration

---

*Documentation v1.0 — generated 2026-04-15*  
*For the full Product Requirements Document, see [docs/prd.md](prd.md)*
