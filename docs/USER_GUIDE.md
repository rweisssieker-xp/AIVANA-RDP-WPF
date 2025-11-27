# Aivana RDP WPF - User Guide

**Version:** 1.0  
**Last Updated:** 2025-11-27

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Connection Management](#connection-management)
3. [Connecting to Remote Desktops](#connecting-to-remote-desktops)
4. [Organizing Connections](#organizing-connections)
5. [File Transfer](#file-transfer)
6. [Clipboard Synchronization](#clipboard-synchronization)
7. [Performance Monitoring](#performance-monitoring)
8. [Session Recording](#session-recording)
9. [Settings & Customization](#settings--customization)
10. [Keyboard Shortcuts](#keyboard-shortcuts)
11. [Troubleshooting](#troubleshooting)
12. [FAQ](#faq)

---

## Getting Started

### Installation

1. Download the installer from the [Releases](https://github.com/yourusername/Aivana-RDP-WPF/releases) page
2. Run the installer and follow the setup wizard
3. Launch Aivana RDP WPF from the Start menu

### First Launch

On first launch, you'll see an empty connection list. The application is ready to use immediately.

### System Requirements

- Windows 10 (version 1809+) or Windows 11
- .NET 8.0 Runtime
- Minimum 4GB RAM
- 100MB free disk space

---

## Connection Management

### Creating a New Connection

1. Click **"New Connection"** in the toolbar
2. Fill in the connection details:
   - **Connection Name**: A friendly name for this connection (e.g., "Production Server")
   - **Server Address**: IP address or hostname (e.g., `192.168.1.100` or `server.example.com`)
   - **Port**: RDP port (default: 3389)
   - **Username**: Your Windows username (optional, can be entered at connection time)
   - **Domain**: Windows domain name (if applicable)
   - **Password**: Enter password if you want to save it securely
   - **Save Password**: Check this box to store credentials in Windows Credential Manager
3. Click **"Save"**

### Editing a Connection

1. Find the connection in the list
2. Click **"Edit"** button
3. Modify the connection details
4. Click **"Save"**

### Deleting a Connection

1. Find the connection in the list
2. Click **"Delete"** button
3. Confirm the deletion

**Note:** Deleting a connection does not delete saved credentials. Use Windows Credential Manager to remove saved passwords.

---

## Connecting to Remote Desktops

### Establishing a Connection

1. **From Connection List:**
   - Click **"Connect"** button next to the connection
   - Or double-click the connection card

2. **From Sidebar:**
   - Click the connection name in the sidebar
   - Click **"Connect"** button

3. **Connection Process:**
   - Status shows "Connecting..." with amber indicator
   - Remote desktop appears in the main content area when connected
   - Status changes to "Connected" with green indicator

### Disconnecting

- Click **"Disconnect"** button in the session toolbar
- Or close the session tab (if using multi-session mode)

### Multiple Simultaneous Connections

Aivana RDP WPF supports multiple active connections:

1. Connect to your first server
2. Open a new connection (it appears as a new tab)
3. Switch between sessions using tabs
4. Each session maintains its own state and settings

---

## Organizing Connections

### Groups

Groups help organize connections by purpose or environment.

**Creating a Group:**
1. When creating/editing a connection, enter a **Group Name** (e.g., "Development", "Production")
2. Connections with the same group name are automatically grouped together

**Filtering by Group:**
- Use the **Group** dropdown in the toolbar
- Select a group name to show only connections in that group

### Tags

Tags provide flexible categorization beyond groups.

**Adding Tags:**
1. When creating/editing a connection, tags are stored automatically
2. Tags can be added programmatically or via import

**Filtering by Tag:**
- Use the **Tag** dropdown in the toolbar
- Select a tag to show all connections with that tag

**Example Tags:**
- `Linux`, `Windows`, `Server2019`
- `Database`, `WebServer`, `FileServer`
- `Critical`, `Test`, `Production`

### Favorites

Mark frequently used connections as favorites for quick access.

**Marking as Favorite:**
- Click the star icon ⭐ next to a connection name
- Filled star = favorite, empty star = not favorite

**Viewing Favorites:**
- Click **"⭐ Favorites"** button in the toolbar
- Only favorite connections are displayed

### Search

Use the search box to quickly find connections:

- Searches in: Connection Name, Server Address, Username, Group Name, Tags
- Search is case-insensitive
- Results update as you type

---

## File Transfer

### Uploading Files

1. **Start a file transfer:**
   - Right-click in the RDP session (context menu)
   - Select "Upload File" (feature in development)
   - Or use the File Transfer service programmatically

2. **Select files:**
   - Choose one or multiple files
   - Click "Upload"

3. **Monitor progress:**
   - Transfer progress is shown in the transfer history
   - Pause/Resume/Cancel options available

### Downloading Files

1. **Initiate download:**
   - Right-click file in remote session
   - Select "Download" (feature in development)

2. **Choose destination:**
   - Select local folder
   - Click "Save"

### Transfer History

- View all file transfers in the transfer history panel
- See transfer status (Completed, Failed, In Progress)
- Resume interrupted transfers
- View transfer statistics

**Note:** File Transfer currently uses simulation mode. Full RDP Virtual Channels integration is planned for future releases.

---

## Clipboard Synchronization

### Enabling Clipboard Sync

Clipboard synchronization is enabled by default for all connections.

**To disable:**
1. Open connection settings
2. Uncheck "Enable Clipboard Synchronization"
3. Save settings

### How It Works

- **Copy** text or files on local machine → Available in remote session
- **Copy** text or files in remote session → Available on local machine
- Supports multiple formats: Text, Images, File Lists

### Clipboard History

- View clipboard history for each connection
- Select previous clipboard entries
- History is maintained per connection (last 50 items)

**Note:** Clipboard synchronization currently supports basic text and image formats. Full format preservation is in development.

---

## Performance Monitoring

### Viewing Connection Health

1. **Connect to a remote desktop**
2. **Click "Health Metrics"** button in the session toolbar
3. **View real-time metrics:**
   - **Network**: Latency, Bandwidth, Packet Loss
   - **Quality**: Frame Rate, Quality Score
   - **Resources**: CPU Usage, Memory Usage, Network Usage

### Understanding Metrics

- **Latency**: Round-trip time in milliseconds (lower is better)
- **Bandwidth**: Available network bandwidth in Mbps
- **Frame Rate**: Frames per second (higher is better, 30+ is good)
- **Quality Score**: Overall connection quality (0-100, 80+ is excellent)

### Performance Alerts

The application automatically alerts you when:
- Latency exceeds threshold (configurable)
- Packet loss is detected
- Quality score drops below acceptable level

---

## Session Recording

### Starting a Recording

1. **Connect to a remote desktop**
2. **Start recording** (feature in development)
3. **Recording indicator** appears in the session toolbar

### Recording Controls

- **Pause**: Temporarily pause recording
- **Resume**: Continue recording
- **Stop**: End recording and save file

### Recording Management

- View all recordings in the recording history
- Playback recordings
- Export recordings in various formats
- Delete old recordings

**Note:** Session recording currently creates placeholder files. Full video encoding with Windows Media Foundation is planned.

---

## Settings & Customization

### Theme Settings

**Changing Theme:**
1. Click **Settings** in the main menu
2. Select **Theme** section
3. Choose **Light** or **Dark** theme
4. Theme changes immediately

### Application Settings

Access settings via **Settings** menu:

- **Default Connection Settings**: Default port, resolution, color depth
- **Performance Settings**: Max concurrent connections, timeout values
- **Update Preferences**: Auto-update settings

### Connection-Specific Settings

Each connection can have custom settings:

- **Resolution**: Desktop resolution (1920x1080, 1680x1050, etc.)
- **Color Depth**: 16-bit, 24-bit, or 32-bit
- **Advanced Options**: Compression, bitmap caching, auto-reconnect

---

## Keyboard Shortcuts

### General

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New Connection |
| `Ctrl+F` | Focus Search Box |
| `Ctrl+R` | Refresh Connection List |
| `F5` | Refresh |
| `Ctrl+,` | Open Settings |
| `F1` | Help |

### Connection Management

| Shortcut | Action |
|----------|--------|
| `Enter` | Connect to selected connection |
| `Delete` | Delete selected connection |
| `F2` | Edit selected connection |
| `Ctrl+E` | Export connections |
| `Ctrl+I` | Import connections |

### Session Management

| Shortcut | Action |
|----------|--------|
| `Ctrl+T` | New Session Tab |
| `Ctrl+W` | Close Current Tab |
| `Ctrl+Tab` | Switch to Next Tab |
| `Ctrl+Shift+Tab` | Switch to Previous Tab |
| `Ctrl+1-9` | Switch to Tab Number |

### During RDP Session

| Shortcut | Action |
|----------|--------|
| `Ctrl+Alt+End` | Send Ctrl+Alt+Del to Remote |
| `F11` | Toggle Full Screen |
| `Alt+Enter` | Toggle Full Screen |

---

## Troubleshooting

### Connection Issues

**Problem: Cannot connect to remote desktop**

**Solutions:**
1. Verify server address and port are correct
2. Check if RDP is enabled on remote machine
3. Verify firewall allows RDP connections (port 3389)
4. Check network connectivity (ping the server)
5. Verify credentials are correct

**Problem: Connection times out**

**Solutions:**
1. Check network connectivity
2. Verify firewall settings
3. Increase connection timeout in settings
4. Check if remote machine is powered on

**Problem: "Credential Manager Error"**

**Solutions:**
1. Run application as Administrator (if needed)
2. Check Windows Credential Manager permissions
3. Manually delete credentials from Windows Credential Manager
4. Re-enter credentials in connection settings

### Performance Issues

**Problem: Slow connection or high latency**

**Solutions:**
1. Check network bandwidth
2. Reduce color depth (16-bit instead of 32-bit)
3. Enable compression in connection settings
4. Close other bandwidth-intensive applications
5. Check remote server performance

**Problem: High CPU usage**

**Solutions:**
1. Reduce number of simultaneous connections
2. Disable performance monitoring if not needed
3. Close unused session tabs
4. Check for background processes

### Application Issues

**Problem: Application won't start**

**Solutions:**
1. Verify .NET 8.0 Runtime is installed
2. Check Windows Event Viewer for errors
3. Run application as Administrator
4. Reinstall the application

**Problem: Database errors**

**Solutions:**
1. Check application data folder permissions (`%AppData%\Aivana_RDP_WPF`)
2. Delete database file and restart (connections will be lost)
3. Check disk space availability

### Log Files

Log files are located at: `%AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log`

Check log files for detailed error information when troubleshooting.

---

## FAQ

### General Questions

**Q: Is Aivana RDP WPF free?**  
A: Yes, Aivana RDP WPF is open source and free to use.

**Q: Does it work with Linux/Mac?**  
A: No, Aivana RDP WPF is Windows-only. It requires Windows 10 or Windows 11.

**Q: Can I use it commercially?**  
A: Yes, the MIT License allows commercial use.

**Q: How is it different from built-in Windows RDP client?**  
A: Aivana RDP WPF provides modern UI, multi-session management, file transfer, clipboard sync, performance monitoring, and better organization features.

### Connection Questions

**Q: Can I import connections from Windows RDP?**  
A: Yes, use the Import feature and select `.rdp` files.

**Q: Are passwords stored securely?**  
A: Yes, passwords are stored in Windows Credential Manager, which uses Windows security APIs.

**Q: Can I export my connections?**  
A: Yes, use the Export feature to save connections as JSON or CSV files.

**Q: How many simultaneous connections are supported?**  
A: Default is 10 concurrent connections, but this can be configured in settings.

### Feature Questions

**Q: Does file transfer work with all RDP servers?**  
A: File transfer requires RDP Virtual Channels support on the server side. Most Windows servers support this.

**Q: Can I record sessions?**  
A: Session recording is available, but currently creates placeholder files. Full video recording is planned.

**Q: Does clipboard sync work with images?**  
A: Basic image support is available. Full format preservation is in development.

**Q: Can I customize the UI?**  
A: Theme switching (Light/Dark) is available. More customization options are planned.

---

## Getting Help

### Documentation

- **[Developer Guide](DEVELOPER_GUIDE.md)** - For developers
- **[Architecture](architecture.md)** - Technical details
- **[Configuration Guide](CONFIGURATION_GUIDE.md)** - Settings reference
- **[Troubleshooting Guide](TROUBLESHOOTING.md)** - Detailed troubleshooting

### Support Channels

- **GitHub Issues**: Report bugs or request features
- **GitHub Discussions**: Ask questions and share ideas
- **Documentation**: Check the docs folder for detailed guides

---

**Last Updated:** 2025-11-27

