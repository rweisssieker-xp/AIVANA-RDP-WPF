# Aivana RDP WPF — Product Requirements Document (PRD)

**Author:** Product Team  
**Date:** 2026-04-15  
**Version:** 2.0  
**Status:** Active

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Product Vision](#product-vision)
3. [Target Users & Personas](#target-users--personas)
4. [Project Classification](#project-classification)
5. [Success Criteria](#success-criteria)
6. [Product Scope](#product-scope)
7. [Functional Requirements](#functional-requirements)
8. [Non-Functional Requirements](#non-functional-requirements)
9. [Technical Architecture Overview](#technical-architecture-overview)
10. [Roadmap](#roadmap)

---

## Executive Summary

**Aivana RDP WPF** is a modern, feature-rich Windows desktop application that reimagines the Remote Desktop Protocol (RDP) client experience. Built with WPF (.NET 8) and Fluent Design principles, it transforms the outdated standard Windows RDP client into a comprehensive remote access management platform.

The product consolidates advanced features — multi-protocol connectivity (RDP, SSH, VNC), intelligent workflow automation, secure credential management, file transfer, clipboard synchronisation, performance monitoring, session recording, and a rich tabbed workspace — into a single, modern application that feels native to Windows 10/11.

### What Makes This Special

| Differentiator | Description |
|---|---|
| **Multi-Protocol** | Single app for RDP, SSH, and VNC — no tool-switching |
| **Workflow Engine** | Automate complex connection sequences and tasks with a visual workflow editor |
| **Modern UI/UX** | First-class Fluent Design 2.0 — light/dark themes, responsive layout |
| **Wake-on-LAN** | Wake sleeping machines from the app before connecting |
| **SSH Tunnelling** | Automatic SSH tunnel setup for secure connections through jump hosts |
| **Workspace Management** | Tabbed interface with persistent layouts (remote sessions, file explorer, terminal, dashboard) |
| **Security-First** | Windows Credential Manager integration, TLS enforcement, encrypted storage |

---

## Product Vision

> **To be the definitive remote desktop management platform for Windows users — professional-grade power wrapped in a consumer-grade experience.**

Aivana RDP WPF aims to replace fragmented workflows (multiple tools for RDP, SSH, and VNC; separate file transfer clients; manual scripting for repetitive tasks) with one cohesive, elegant application.

---

## Target Users & Personas

### Persona 1 — IT Administrator "Alex"
- Manages 50–500 Windows and Linux servers
- Uses RDP daily, SSH occasionally, struggles with poor tooling
- Needs: quick connection switching, bulk operations, audit logs, Wake-on-LAN
- Pain: existing tools are slow, hard to organise, and lack automation

### Persona 2 — DevOps Engineer "Dana"
- Connects to cloud VMs, jumps through bastion hosts, needs SSH tunnels
- Automates repetitive connection workflows
- Needs: SSH tunnel support, workflow automation, multi-session management
- Pain: manual SSH-tunnel setup every time; no unified view of active sessions

### Persona 3 — Power User "Morgan"
- Works on multiple remote machines simultaneously for development
- Copies files between machines regularly, records sessions for documentation
- Needs: file transfer, clipboard sync, session recording, tabbed workspace
- Pain: standard mstsc.exe is primitive; no clipboard history or file drag-and-drop

### Persona 4 — Small Business Owner "Sam"
- Connects to 2–5 remote PCs for remote support and administration
- Not technical; needs a simple, clean interface
- Needs: saved connection profiles, favourites, one-click connect
- Pain: forgets IP addresses and settings; existing tools too complex

---

## Project Classification

| Attribute | Value |
|---|---|
| **Type** | Windows Desktop Application |
| **Domain** | Remote Access & IT Management |
| **Framework** | .NET 8.0, WPF |
| **UI System** | Fluent Design System 2.0 |
| **Complexity** | Medium |
| **Primary Platform** | Windows 10 (1809+), Windows 11 |
| **Architecture** | x64 |
| **Pattern** | MVVM, Dependency Injection, Service Layer |

### Windows Integration Points
- Windows Credential Manager (secure credential storage)
- Windows Notification System (connection status alerts)
- Windows File Explorer (drag-and-drop file operations)
- Windows Clipboard API (clipboard synchronisation)
- MSTSC ActiveX Control (`mstscax.dll`) for RDP rendering

---

## Success Criteria

### Primary Success Metrics

| Metric | Target |
|---|---|
| Active users (6 months post-launch) | 1,000+ |
| User satisfaction rating | ≥ 4.5 / 5.0 stars |
| Advanced feature adoption | ≥ 70 % of users use 3+ advanced features |
| Connection success rate | ≥ 99.5 % with auto-reconnect |
| Security incidents | 0 credential breaches |

### Performance Targets

| Metric | Target |
|---|---|
| Application startup | < 2 s (SSD + 8 GB RAM) |
| Memory footprint (single session) | < 150 MB |
| Idle CPU usage | < 5 % |
| Connection establishment (LAN) | < 3 s |
| RDP frame rate | ≥ 30 FPS on standard hardware |
| Clipboard sync latency | < 100 ms (text) |
| UI frame rate during animations | 60 FPS |
| Max saved connection profiles | ≥ 100 without degradation |
| Max simultaneous connections | ≥ 10 |

### User Experience Targets
- New user can establish first connection within 2 minutes of installation
- Power users can automate workflows without writing code
- Interface rated "intuitive" and "modern" compared with mstsc.exe

---

## Product Scope

### MVP — Minimum Viable Product (v1.0 — Delivered)

#### Core Connection Features
- RDP protocol support using MSTSC ActiveX Control
- SSH protocol support
- VNC protocol support
- Connection profile management (create, edit, delete)
- Connection groups and tags for organisation
- Favourites for quick access
- Search and filter across all profiles
- Connection history tracking
- Import/Export: `.rdp`, JSON, CSV formats
- Multi-session support with tabbed interface
- Auto-reconnect with configurable retry logic

#### Advanced Features
- **Wake-on-LAN**: Send magic packets before connecting, wait for host availability
- **SSH Tunnelling**: Automatic tunnel creation through jump hosts for secure access
- **Workflow Engine**: Visual automation for multi-step connection tasks
- **Workspace Management**: Tabbed workspace with remote sessions, file explorer, local terminal, dashboard, and settings tabs
- **Quick Connect**: Fast connection without saving a profile
- **Layout Persistence**: Save and restore workspace layouts

#### File Transfer & Clipboard
- File upload/download between local and remote machines
- Transfer history with pause/resume/cancel
- Bidirectional clipboard synchronisation
- Clipboard history (last 50 items)
- Text and image format support

#### Performance & Monitoring
- Real-time network metrics: latency, bandwidth, packet loss
- Quality metrics: frame rate, quality score (0–100), quality level (VeryPoor → Excellent)
- Resource metrics: CPU usage, memory, network usage
- Historical performance data storage

#### Security
- Windows Credential Manager integration (encrypted credential storage)
- TLS 1.2+ enforcement for all network communications
- Session encryption options
- Basic audit logging (connection events, security activities)
- Certificate management UI

#### User Interface
- Fluent Design System 2.0 implementation
- Light and Dark themes with instant switching
- Responsive layout (adapts to window size)
- Keyboard shortcuts for all major actions
- Modern animations and transitions

#### Infrastructure
- SQLite database via Entity Framework Core (profiles, history, metrics)
- Dependency Injection (Microsoft.Extensions.DependencyInjection)
- Structured logging (Microsoft.Extensions.Logging, file + console)
- Configurable via `appsettings.json`

### Growth Features (Post-v1.0)

#### Advanced UI/UX
- Custom theme editor (colour customisation)
- Advanced gesture support for touch devices
- Multi-monitor RDP configuration UI
- Accessibility enhancements: full screen-reader support (Narrator, NVDA, JAWS), WCAG 2.1 AA colours

#### Performance Enhancements
- Hardware-accelerated (GPU) rendering
- Adaptive quality based on network conditions
- Advanced compression algorithms
- Bottleneck detection and alerts

#### Collaboration
- Screen sharing / view-only mode
- Session annotation tools
- Chat integration within sessions
- Session broadcasting

#### Advanced Security
- Multi-Factor Authentication (MFA) support
- Hardware key support (YubiKey, FIDO2)
- IP whitelisting/blacklisting
- VPN integration
- Compliance reporting (SOC 2, ISO 27001)

#### Automation & Integration
- REST API for third-party integration
- PowerShell scripting support
- Macro recording and playback
- Scheduled connections
- Command-line interface (CLI)
- Password manager integration (1Password, Bitwarden)

#### Advanced Peripherals
- USB device redirection
- Audio redirection with quality controls
- Printer redirection
- Session recording to video (Windows Media Foundation)

### Vision (Future)

- **AI-powered**: Network quality prediction, adaptive quality via ML, automated troubleshooting
- **Plugin marketplace**: Extension SDK and community marketplace
- **Cloud sync**: Encrypted cloud backup of connection profiles and settings
- **Team workspaces**: Shared connection libraries for organisations
- **VR/AR visualisation** (experimental): Remote desktop in 3D workspace

---

## Functional Requirements

### FR-CM — Connection Management

| ID | Requirement |
|---|---|
| FR-CM-01 | Users can create, edit, and delete connection profiles with name, server address, port, username, domain, group, and tags |
| FR-CM-02 | Users can assign connections to named groups for organisation |
| FR-CM-03 | Users can tag connections with arbitrary labels for flexible filtering |
| FR-CM-04 | Users can mark connections as favourites and view a favourites-only list |
| FR-CM-05 | Users can search connections by name, server address, username, group, or tags (real-time, case-insensitive) |
| FR-CM-06 | Users can import connection profiles from `.rdp`, JSON, and CSV files |
| FR-CM-07 | Users can export connection profiles to JSON and CSV |
| FR-CM-08 | The system tracks connection count and last-connected timestamp per profile |
| FR-CM-09 | Users can configure default port, colour depth, and resolution per profile |
| FR-CM-10 | Users can enable Wake-on-LAN per profile (MAC address, broadcast address, wake timeout) |
| FR-CM-11 | Users can enable SSH tunnel per profile (SSH host, port, credentials, local/remote port mapping) |
| FR-CM-12 | Users can select the protocol per profile (RDP, SSH, VNC) |

### FR-CN — Connection & Session

| ID | Requirement |
|---|---|
| FR-CN-01 | Users can connect to and disconnect from remote desktops |
| FR-CN-02 | The system supports RDP connections using the MSTSC ActiveX Control |
| FR-CN-03 | The system supports SSH connections |
| FR-CN-04 | The system supports VNC connections |
| FR-CN-05 | The system supports ≥10 simultaneous active sessions in a tabbed interface |
| FR-CN-06 | Session status is reflected in real time (Connecting, Connected, Disconnected, Suspended, Error, Reconnecting) |
| FR-CN-07 | Users can toggle full-screen mode per session |
| FR-CN-08 | The system tracks session duration, connect/disconnect timestamps, and session events |
| FR-CN-09 | The system attempts auto-reconnect on dropped connections with configurable retry |
| FR-CN-10 | Users can perform a Quick Connect without saving a profile |

### FR-WS — Workspace Management

| ID | Requirement |
|---|---|
| FR-WS-01 | The workspace supports multiple tab types: RemoteSession, LocalTerminal, FileExplorer, Settings, Dashboard, Custom |
| FR-WS-02 | Each tab has a title, icon, status indicator, and close button |
| FR-WS-03 | Users can save and restore workspace layouts (open tabs, positions) |
| FR-WS-04 | The last-activated timestamp is tracked per tab for navigation history |
| FR-WS-05 | Tabs can be individually closable or pinned |

### FR-WF — Workflow Engine

| ID | Requirement |
|---|---|
| FR-WF-01 | Users can create named workflow definitions with an ordered list of steps |
| FR-WF-02 | Workflows support step types: ConnectRemote, DisconnectRemote, StartApplication, StopApplication, TransferFile, ExecuteCommand, SendKeystrokes, TakeScreenshot, WaitCondition, ShowNotification, LogMessage, SetVariable, IfCondition, LoopSteps, ParallelSteps, Delay, CustomScript |
| FR-WF-03 | Workflows support triggers: Manual, Scheduled, OnConnection, OnDisconnection, OnFileChange, OnSystemEvent, OnHotkey, OnApplicationStart |
| FR-WF-04 | Each step has configurable parameters, timeout, retry count, and an optional conditional |
| FR-WF-05 | Conditions support operators: Equals, NotEquals, GreaterThan, LessThan, Contains, StartsWith, EndsWith, IsNull, IsNotNull |
| FR-WF-06 | Workflows can define and pass variables between steps |
| FR-WF-07 | Workflow execution history and step results are tracked |
| FR-WF-08 | Users can enable, disable, pause, and delete workflows |

### FR-WOL — Wake-on-LAN

| ID | Requirement |
|---|---|
| FR-WOL-01 | Users can trigger Wake-on-LAN for any connection profile that has a MAC address configured |
| FR-WOL-02 | The system sends a magic packet to the configurable broadcast address |
| FR-WOL-03 | The system waits for host availability after sending the wake packet (configurable timeout) |
| FR-WOL-04 | The system validates MAC address format before sending |

### FR-SSH — SSH Tunnelling

| ID | Requirement |
|---|---|
| FR-SSH-01 | Users can configure an SSH tunnel for any connection profile |
| FR-SSH-02 | The system creates the tunnel before establishing the remote connection |
| FR-SSH-03 | Users can view all active tunnels and close individual tunnels |
| FR-SSH-04 | Tunnel status (active / inactive) is queryable at runtime |

### FR-FT — File Transfer

| ID | Requirement |
|---|---|
| FR-FT-01 | Users can initiate file uploads from local machine to remote desktop |
| FR-FT-02 | Users can initiate file downloads from remote desktop to local machine |
| FR-FT-03 | Multiple concurrent file transfers are supported |
| FR-FT-04 | Users can pause, resume, and cancel individual transfers |
| FR-FT-05 | Transfer history is maintained (status, size, speed, timestamps) |
| FR-FT-06 | Transfer progress is displayed in real time |

### FR-CB — Clipboard Synchronisation

| ID | Requirement |
|---|---|
| FR-CB-01 | Clipboard content is synchronised bidirectionally between local machine and remote session |
| FR-CB-02 | Supported formats: text, images, file lists |
| FR-CB-03 | Clipboard history is maintained per session (last 50 items) |
| FR-CB-04 | Users can enable or disable clipboard sync per connection |
| FR-CB-05 | Users can browse clipboard history and paste previous entries |

### FR-PM — Performance Monitoring

| ID | Requirement |
|---|---|
| FR-PM-01 | The system collects real-time metrics: latency (ms), bandwidth (Mbps), packet loss (%), frame rate (FPS), quality score (0–100), CPU (%), memory (MB), network usage (Mbps) |
| FR-PM-02 | Connection quality is derived from quality score: VeryPoor (<40), Poor (40–59), Fair (60–74), Good (75–89), Excellent (≥90) |
| FR-PM-03 | Historical performance data is persisted per connection profile |
| FR-PM-04 | The system can alert users when thresholds are exceeded (high latency, packet loss, low quality) |

### FR-SR — Session Recording

| ID | Requirement |
|---|---|
| FR-SR-01 | Users can start, pause, resume, and stop session recordings |
| FR-SR-02 | Recordings are stored to local disk with configurable output path |
| FR-SR-03 | Recording history is maintained (duration, file size, timestamps) |
| FR-SR-04 | Users can delete recordings from the history view |

### FR-SEC — Security & Credentials

| ID | Requirement |
|---|---|
| FR-SEC-01 | Credentials (username, password) are stored in Windows Credential Manager |
| FR-SEC-02 | The system retrieves and uses stored credentials automatically at connection time |
| FR-SEC-03 | Users can explicitly save, update, and delete stored credentials |
| FR-SEC-04 | Connection events are logged for audit purposes |
| FR-SEC-05 | Session data is never stored in plain-text logs |

### FR-UI — User Interface & Experience

| ID | Requirement |
|---|---|
| FR-UI-01 | Users can switch between Light and Dark themes |
| FR-UI-02 | The layout responds to window resising |
| FR-UI-03 | All major features are accessible via keyboard shortcuts |
| FR-UI-04 | The application provides smooth animations and visual feedback |
| FR-UI-05 | Connection status is indicated with colour-coded indicators |
| FR-UI-06 | Users can access application-wide settings (defaults, theme, logging) |

### FR-CFG — Settings & Configuration

| ID | Requirement |
|---|---|
| FR-CFG-01 | Default connection settings (port, colour depth, resolution) are configurable globally |
| FR-CFG-02 | Maximum concurrent connections is configurable (default: 10) |
| FR-CFG-03 | Connection timeout is configurable (default: 30 000 ms) |
| FR-CFG-04 | Theme preference (Light, Dark, System) is persisted |
| FR-CFG-05 | Logging level is configurable per category |
| FR-CFG-06 | Auto-update and update-check frequency is configurable |

---

## Non-Functional Requirements

### Performance

| ID | Requirement |
|---|---|
| NFR-P-01 | Application startup in < 2 s on SSD hardware with ≥ 8 GB RAM |
| NFR-P-02 | Memory footprint ≤ 150 MB for a single active connection |
| NFR-P-03 | CPU usage ≤ 5 % when idle (no active connections) |
| NFR-P-04 | Connection establishment ≤ 3 s on LAN |
| NFR-P-05 | RDP frame rate ≥ 30 FPS on standard hardware |
| NFR-P-06 | Clipboard sync latency ≤ 100 ms for text |
| NFR-P-07 | UI animations at 60 FPS |
| NFR-P-08 | Application remains responsive during background file transfers |
| NFR-P-09 | File transfer bandwidth utilisation ≥ 80 % on stable connections |

### Security

| ID | Requirement |
|---|---|
| NFR-S-01 | All credentials encrypted at rest via Windows Credential Manager |
| NFR-S-02 | All network communications use TLS 1.2 or higher |
| NFR-S-03 | Invalid SSL/TLS certificates trigger a user warning |
| NFR-S-04 | Session data not stored in plain-text logs |
| NFR-S-05 | Application follows Windows security best practices |
| NFR-S-06 | Audit logs are timestamped, include user identification, and are tamper-evident |

### Scalability

| ID | Requirement |
|---|---|
| NFR-SC-01 | ≥ 100 saved connection profiles without performance degradation |
| NFR-SC-02 | ≥ 10 simultaneous active connections |
| NFR-SC-03 | File transfers support files up to 10 GB |
| NFR-SC-04 | Session recording up to 8 hours |
| NFR-SC-05 | Graceful handling of network interruptions without data loss |

### Accessibility

| ID | Requirement |
|---|---|
| NFR-A-01 | Compatible with Windows high contrast mode |
| NFR-A-02 | Compatible with screen readers (Narrator, NVDA, JAWS) |
| NFR-A-03 | All UI elements keyboard-accessible |
| NFR-A-04 | Supports Windows accessibility settings (text scaling, colour filters) |
| NFR-A-05 | Colour contrast ratios meet WCAG 2.1 AA standards |

### Reliability & Maintainability

| ID | Requirement |
|---|---|
| NFR-R-01 | Connection success rate ≥ 99.5 % with auto-reconnect |
| NFR-R-02 | Application logs all errors with stack traces for diagnostics |
| NFR-R-03 | Log files rotate daily and are retained for 30 days |
| NFR-R-04 | Database migrations are applied automatically on startup |
| NFR-R-05 | Unit test coverage ≥ 70 % for service layer |

### Integration

| ID | Requirement |
|---|---|
| NFR-I-01 | Import/export compatibility with standard `.rdp` file format |
| NFR-I-02 | Integration with Windows File Explorer for drag-and-drop |
| NFR-I-03 | Integration with Windows Notification System |
| NFR-I-04 | Integration with Windows Print Spooler for printer redirection (growth) |
| NFR-I-05 | REST API for third-party integration (growth) |

---

## Technical Architecture Overview

### Technology Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8.0 |
| UI Framework | WPF (Windows Presentation Foundation) |
| UI Pattern | MVVM with CommunityToolkit.Mvvm 8.2.2 |
| Database | SQLite via Entity Framework Core 8.0 |
| RDP Client | MSTSC ActiveX Control (mstscax.dll / MSTSCLib) |
| Credential Storage | Windows Credential Manager API |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Logging | Microsoft.Extensions.Logging (Console + File) |
| Configuration | Microsoft.Extensions.Configuration (JSON) |
| Protocols | RDP (MSTSC), SSH, VNC (via protocol abstraction layer) |

### Key Architectural Decisions

1. **MVVM pattern** enforces separation of UI and business logic — Views contain zero business logic.
2. **Service layer with interfaces** (`IXxxService`) allows unit testing through mocking and future implementation swaps.
3. **Protocol abstraction** (`IRemoteProtocol`, `IProtocolFactory`, `ProtocolType`) enables adding new protocols without changing core connection logic.
4. **Workflow engine** implements a declarative step-based automation model with typed step parameters, conditional execution, loops, and parallel steps.
5. **SQLite** for local-first persistence — no server dependency, auto-migrated via EF Core.

### Project Structure

```
Aivana_RDP_WPF/
├── Models/             # EF Core entities & domain models
├── ViewModels/         # MVVM ViewModels (CommunityToolkit.Mvvm)
├── Views/              # WPF XAML views
├── Services/           # Business logic (interfaces + implementations)
├── Infrastructure/
│   ├── Database/       # ApplicationDbContext, migrations factory
│   ├── Rdp/            # RdpClientWrapper (MSTSC ActiveX)
│   ├── Credentials/    # WindowsCredentialManager
│   ├── Logging/        # File logger
│   └── Protocols/      # Protocol abstraction (RDP, SSH, VNC)
├── Helpers/            # Utility classes (validation, file, network, tags)
├── Converters/         # WPF value converters
├── Commands/           # RelayCommand, AsyncRelayCommand
├── Resources/
│   ├── Styles/         # FluentDesignStyles.xaml
│   └── Themes/         # LightTheme.xaml, DarkTheme.xaml
└── Migrations/         # EF Core database migrations
```

---

## Roadmap

### v1.0 (Released — 2025-11-27)
- ✅ Multi-protocol connectivity (RDP, SSH, VNC)
- ✅ Connection profile management (CRUD, groups, tags, favourites)
- ✅ Wake-on-LAN
- ✅ SSH Tunnelling
- ✅ Workflow Engine
- ✅ Workspace tab management with layout persistence
- ✅ Quick Connect
- ✅ File transfer (simulation mode)
- ✅ Clipboard synchronisation (basic)
- ✅ Performance monitoring (local metrics)
- ✅ Session recording (placeholder files)
- ✅ Fluent Design UI (Light/Dark themes)
- ✅ Import/Export (RDP, JSON, CSV)
- ✅ SQLite persistence with EF Core
- ✅ Structured logging

### v1.1 (Near-term)
- [ ] Full file transfer via RDP Virtual Channels
- [ ] Full clipboard format preservation (binary formats, files)
- [ ] Session recording to video (Windows Media Foundation)
- [ ] Enhanced performance dashboard with historical graphs
- [ ] Multi-monitor RDP configuration UI

### v1.2 (Mid-term)
- [ ] REST API for third-party integration
- [ ] PowerShell CLI integration
- [ ] Advanced workflow trigger types (file system, hotkey)
- [ ] Password manager integration (Bitwarden, 1Password)
- [ ] Accessibility improvements (full screen-reader compliance)

### v2.0 (Long-term)
- [ ] Plugin/extension SDK and marketplace
- [ ] Cloud sync for connection profiles
- [ ] Collaboration features (view-only mode, session sharing)
- [ ] AI-powered adaptive quality and troubleshooting suggestions
- [ ] MFA and hardware key support (YubiKey, FIDO2)

---

*PRD v2.0 — based on analysis of the implemented codebase as of 2026-04-15.*  
*Supersedes PRD v1.0 (2025-11-26).*
