# Aivana RDP WPF

Modern, feature-rich Remote Desktop Protocol (RDP) client for Windows built with WPF and Fluent Design principles.

![Build Status](https://img.shields.io/badge/build-success-brightgreen)
![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows-blue)

---

## 🚀 Features

### Core Functionality
- **Multi-Session Management** - Manage multiple RDP connections simultaneously with a tabbed interface
- **Connection Profile Management** - Create, edit, and organize connection profiles with groups, tags, and favorites
- **Secure Credential Storage** - Windows Credential Manager integration for secure password storage
- **Import/Export** - Import from RDP, JSON, CSV files and export profiles for backup/sharing

### Advanced Features
- **File Transfer** - Upload and download files between local and remote machines
- **Clipboard Synchronization** - Bidirectional clipboard sync with format preservation
- **Performance Monitoring** - Real-time connection health metrics (latency, bandwidth, CPU, memory)
- **Session Recording** - Record RDP sessions for documentation and training
- **Theme Switching** - Light and Dark themes with Fluent Design System 2.0

### User Experience
- **Modern UI** - Fluent Design System implementation for Windows 11
- **Search & Filter** - Quickly find connections by name, server, group, or tags
- **Connection Health Dashboard** - Visual metrics display for connection quality
- **Responsive Design** - Adapts to different window sizes

---

## 📋 Requirements

- **Windows 10** (version 1809 or later) or **Windows 11**
- **.NET 8.0 Runtime** (included in Windows 11, download for Windows 10)
- **Visual C++ Redistributable** (usually pre-installed)

---

## 🛠️ Installation

### Option 1: Build from Source

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/Aivana-RDP-WPF.git
   cd Aivana-RDP-WPF
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project Aivana_RDP_WPF/Aivana_RDP_WPF.csproj
   ```

### Option 2: Installer (Coming Soon)

Download the installer from the [Releases](https://github.com/yourusername/Aivana-RDP-WPF/releases) page and follow the installation wizard.

---

## 🎯 Quick Start

### First Connection

1. **Launch Aivana RDP WPF**
2. **Click "New Connection"** in the toolbar
3. **Fill in connection details:**
   - Connection Name (e.g., "Production Server")
   - Server Address (IP or hostname)
   - Port (default: 3389)
   - Username (optional)
   - Domain (optional)
4. **Click "Save"**
5. **Click "Connect"** in the connection list or sidebar

### Organizing Connections

- **Groups**: Assign connections to groups (e.g., "Development", "Production")
- **Tags**: Add tags for flexible categorization (e.g., "Linux", "Windows", "Database")
- **Favorites**: Click the star icon ⭐ to mark frequently used connections

### Using Multiple Sessions

1. **Connect to multiple servers** simultaneously
2. **Switch between sessions** using the tabbed interface
3. **Monitor performance** for each active connection
4. **Transfer files** or sync clipboard between sessions

---

## 📚 Documentation

- **[User Guide](docs/USER_GUIDE.md)** - Complete user documentation with step-by-step guides
- **[Developer Guide](docs/DEVELOPER_GUIDE.md)** - Setup and development workflow
- **[Architecture](docs/architecture.md)** - Technical architecture and design decisions
- **[Configuration Guide](docs/CONFIGURATION_GUIDE.md)** - Configuration options and settings
- **[Troubleshooting](docs/TROUBLESHOOTING.md)** - Common issues and solutions

---

## 🏗️ Project Structure

```
Aivana-RDP-WPF/
├── Aivana_RDP_WPF/          # Main WPF application
│   ├── Models/              # Data models
│   ├── ViewModels/          # MVVM ViewModels
│   ├── Views/               # WPF Views (XAML)
│   ├── Services/            # Business logic services
│   ├── Infrastructure/      # Database, RDP, Credentials
│   ├── Helpers/             # Utility classes
│   └── Resources/           # Styles, Themes
├── Aivana_RDP_WPF.Tests/   # Unit and integration tests
└── docs/                    # Documentation
```

---

## 🧪 Development

### Prerequisites

- **.NET 8.0 SDK**
- **Visual Studio 2022** (with WPF workload) or **VS Code** with C# extension
- **Windows SDK** (for Windows-specific APIs)

### Building

```bash
# Build solution
dotnet build Aivana_RDP_WPF.sln

# Run tests
dotnet test Aivana_RDP_WPF.Tests/Aivana_RDP_WPF.Tests.csproj
```

### Code Style

- Follow C# coding conventions
- Use async/await for all I/O operations
- XML documentation comments for public APIs
- MVVM pattern for UI code

See [Developer Guide](docs/DEVELOPER_GUIDE.md) for detailed development instructions.

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines.

### Quick Contribution Steps

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/) and [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- Uses [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for MVVM support
- Fluent Design System implementation inspired by Microsoft's design guidelines

---

## 📞 Support

- **Documentation**: See [docs/](docs/) folder
- **Issues**: [GitHub Issues](https://github.com/yourusername/Aivana-RDP-WPF/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/Aivana-RDP-WPF/discussions)

---

## 🗺️ Roadmap

- [ ] Enhanced file transfer with drag-and-drop UI
- [ ] Advanced clipboard history management
- [ ] Multi-monitor support configuration
- [ ] Session recording with video encoding
- [ ] Plugin system for extensibility
- [ ] Cloud sync for connection profiles

---

**Made with ❤️ for Windows users who need a modern RDP experience**

