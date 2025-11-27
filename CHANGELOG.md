# Changelog

All notable changes to Aivana RDP WPF will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Enhanced file transfer with drag-and-drop UI
- Advanced clipboard history management
- Multi-monitor support configuration
- Session recording with video encoding
- Plugin system for extensibility
- Cloud sync for connection profiles

---

## [1.0.0] - 2025-11-27

### Added

#### Core Features
- **Connection Management**
  - Create, edit, and delete connection profiles
  - Groups and tags for organization
  - Favorites for quick access
  - Search and filter functionality
  - Import/Export (RDP, JSON, CSV formats)

- **RDP Connection**
  - Establish RDP connections using MSTSC ActiveX Control
  - Multi-session support with tabbed interface
  - Connection status tracking
  - Connect/Disconnect functionality

- **Security**
  - Windows Credential Manager integration
  - Secure password storage
  - Credential management per connection

- **File Transfer**
  - Upload files to remote desktop
  - Download files from remote desktop
  - Transfer history tracking
  - Pause/Resume/Cancel transfers
  - Progress monitoring

- **Clipboard Synchronization**
  - Bidirectional clipboard sync
  - Clipboard history (last 50 items)
  - Enable/Disable per connection
  - Text and image format support

- **Performance Monitoring**
  - Real-time connection health metrics
  - Network metrics (latency, bandwidth, packet loss)
  - Quality metrics (frame rate, quality score)
  - Resource usage (CPU, memory, network)
  - Historical performance data

- **Session Recording**
  - Start/Stop/Pause/Resume recording
  - Recording management
  - Recording history

- **User Interface**
  - Fluent Design System 2.0 implementation
  - Light and Dark themes
  - Responsive layout
  - Modern, intuitive UI
  - Multi-session tabbed interface

#### Infrastructure
- **Database**
  - SQLite database with Entity Framework Core
  - Connection profile persistence
  - Session history tracking
  - Database migrations support

- **Logging**
  - Structured logging with Microsoft.Extensions.Logging
  - Console and file logging
  - Configurable log levels
  - Log file rotation and retention

- **Configuration**
  - appsettings.json configuration
  - User-specific settings override
  - Environment variable support
  - Flexible configuration system

- **Services**
  - Dependency Injection with Microsoft.Extensions.DependencyInjection
  - Service layer architecture
  - MVVM pattern implementation
  - Command pattern for UI actions

### Technical Details

#### Architecture
- **Framework**: .NET 8.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Pattern**: MVVM (Model-View-ViewModel)
- **Database**: SQLite with Entity Framework Core
- **RDP**: MSTSC ActiveX Control (mstscax.dll)
- **Credential Storage**: Windows Credential Manager API

#### Project Structure
- Clean separation of concerns
- Modular service architecture
- Comprehensive helper utilities
- Value converters for UI binding
- Resource dictionaries for styling

### Known Limitations

#### Simulation/Stub Functionality
Some features use simulation instead of full RDP integration:

1. **File Transfer Service**
   - Simulates transfer progress
   - Full RDP Virtual Channels integration planned

2. **Clipboard Service**
   - Basic implementation
   - Full format preservation in development

3. **Performance Monitor Service**
   - Measures local metrics
   - Remote metrics integration planned

4. **Session Recording Service**
   - Creates placeholder files
   - Windows Media Foundation integration planned

### Documentation
- Complete PRD (Product Requirements Document)
- Architecture specification with ADRs
- UX Design specification
- Epics and Stories breakdown
- Technical specifications for all epics
- Implementation status documentation
- User Guide
- Developer Guide
- Configuration Guide
- Troubleshooting Guide
- Contributing Guide

### Testing
- Unit test infrastructure
- Integration test framework
- System test setup
- Performance test framework
- Security test framework

---

## Version History

### Version 1.0.0 (2025-11-27)
- Initial release
- Core RDP connection functionality
- Connection management features
- File transfer (simulation mode)
- Clipboard synchronization (basic)
- Performance monitoring (local metrics)
- Session recording (placeholder)
- Modern UI with Fluent Design
- Complete documentation

---

## Release Notes Format

### [Version] - YYYY-MM-DD

#### Added
- New features

#### Changed
- Changes to existing functionality

#### Deprecated
- Soon-to-be removed features

#### Removed
- Removed features

#### Fixed
- Bug fixes

#### Security
- Security fixes

---

**For detailed feature descriptions, see [User Guide](docs/USER_GUIDE.md)**

**For technical details, see [Architecture Document](docs/architecture.md)**

