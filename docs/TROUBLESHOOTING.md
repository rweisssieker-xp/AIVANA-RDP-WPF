# Aivana RDP WPF - Troubleshooting Guide

**Version:** 1.0  
**Last Updated:** 2025-11-27

---

## Table of Contents

1. [Connection Issues](#connection-issues)
2. [Application Startup Problems](#application-startup-problems)
3. [Performance Issues](#performance-issues)
4. [Database Issues](#database-issues)
5. [Credential Management Issues](#credential-management-issues)
6. [UI/Display Issues](#uidisplay-issues)
7. [File Transfer Issues](#file-transfer-issues)
8. [Clipboard Sync Issues](#clipboard-sync-issues)
9. [Log File Analysis](#log-file-analysis)
10. [Known Issues](#known-issues)

---

## Connection Issues

### Cannot Connect to Remote Desktop

**Symptoms:**
- Connection attempt fails immediately
- "Connection failed" error message
- Connection times out

**Possible Causes & Solutions:**

#### 1. Server Address or Port Incorrect

**Solution:**
- Verify server address (IP or hostname)
- Check port number (default: 3389)
- Test connectivity: `ping <server-address>`
- Test port: `telnet <server-address> 3389` (if telnet enabled)

#### 2. RDP Not Enabled on Remote Machine

**Solution:**
- On remote Windows machine: Settings → System → Remote Desktop → Enable
- Verify firewall allows RDP (port 3389)
- Check Group Policy settings

#### 3. Network Connectivity Issues

**Solution:**
- Check network connection
- Verify VPN connection (if required)
- Test with built-in Windows RDP client to isolate issue
- Check firewall rules (Windows Firewall, corporate firewall)

#### 4. Credentials Incorrect

**Solution:**
- Verify username and password
- Check domain name (if using domain authentication)
- Try connecting without saved credentials
- Clear saved credentials and re-enter

#### 5. Firewall Blocking Connection

**Solution:**
- Windows Firewall: Allow RDP (port 3389)
- Corporate firewall: Contact IT administrator
- Router firewall: Port forwarding may be required

**Check Windows Firewall:**
```powershell
# Check if RDP is allowed
Get-NetFirewallRule -DisplayName "*Remote Desktop*"
```

---

### Connection Times Out

**Symptoms:**
- Connection attempt hangs
- "Connection timeout" error after 30 seconds
- No response from server

**Solutions:**

1. **Increase Timeout:**
   - Edit `appsettings.json`:
   ```json
   {
     "Application": {
       "Performance": {
         "ConnectionTimeout": 60000  // 60 seconds
       }
     }
   }
   ```

2. **Check Network Latency:**
   - Ping the server: `ping <server-address>`
   - High latency (>100ms) may cause timeouts

3. **Verify Server Status:**
   - Ensure remote machine is powered on
   - Check if RDP service is running on remote machine

4. **Network Path Issues:**
   - Traceroute to identify network hops
   - Check for network congestion

---

### "Credential Manager Error"

**Symptoms:**
- Error when saving/loading credentials
- "Access denied" error
- Credentials not persisting

**Solutions:**

1. **Run as Administrator:**
   - Right-click application → Run as Administrator
   - Required for Credential Manager access in some configurations

2. **Check Credential Manager Permissions:**
   - Open Windows Credential Manager
   - Verify you can access stored credentials
   - Manually delete problematic credentials

3. **Clear Credentials:**
   ```powershell
   # List credentials
   cmdkey /list
   
   # Delete specific credential
   cmdkey /delete:Aivana_RDP_WPF_Profile_<ID>
   ```

4. **Re-enter Credentials:**
   - Delete connection profile
   - Create new connection
   - Re-enter credentials

---

## Application Startup Problems

### Application Won't Start

**Symptoms:**
- No window appears
- Application crashes immediately
- Error dialog on startup

**Solutions:**

1. **Check .NET Runtime:**
   ```bash
   dotnet --version
   # Should output: 8.0.x
   ```
   - Install .NET 8.0 Runtime if missing

2. **Check Windows Event Viewer:**
   - Open Event Viewer → Windows Logs → Application
   - Look for error entries related to Aivana RDP WPF
   - Note error codes and messages

3. **Run from Command Line:**
   ```bash
   cd Aivana_RDP_WPF
   dotnet run
   ```
   - Check for error messages in console

4. **Check Dependencies:**
   - Verify all DLLs are present
   - Check for missing Visual C++ Redistributable
   - Reinstall application if needed

---

### "Database Error" on Startup

**Symptoms:**
- "Unable to open database file" error
- Database-related exceptions

**Solutions:**

1. **Check Directory Permissions:**
   - Verify `%AppData%\Aivana_RDP_WPF` exists
   - Check write permissions
   - Create directory manually if missing:
   ```powershell
   New-Item -ItemType Directory -Path "$env:APPDATA\Aivana_RDP_WPF" -Force
   ```

2. **Check Disk Space:**
   - Ensure sufficient disk space available
   - SQLite requires free space for database operations

3. **Reset Database:**
   ```bash
   # Delete database file
   Remove-Item "$env:APPDATA\Aivana_RDP_WPF\aivana.db"
   
   # Recreate database
   cd Aivana_RDP_WPF
   dotnet ef database update
   ```
   **Warning:** This deletes all connection profiles!

4. **Check Database File Lock:**
   - Ensure no other process is accessing database
   - Close other instances of application
   - Check for database backup tools accessing file

---

## Performance Issues

### Slow Connection or High Latency

**Symptoms:**
- Laggy remote desktop experience
- High latency metrics
- Slow response to input

**Solutions:**

1. **Reduce Color Depth:**
   - Edit connection → Display Settings
   - Change from 32-bit to 16-bit color depth
   - Reduces bandwidth usage

2. **Enable Compression:**
   - Connection settings → Advanced
   - Enable compression
   - Reduces network traffic

3. **Check Network Bandwidth:**
   - Run speed test
   - Close bandwidth-intensive applications
   - Check for network congestion

4. **Reduce Resolution:**
   - Lower desktop resolution (e.g., 1280x720 instead of 1920x1080)
   - Reduces data transfer

5. **Close Unused Sessions:**
   - Each active session consumes resources
   - Close sessions not in use

---

### High CPU Usage

**Symptoms:**
- Application uses high CPU (>50%)
- System becomes sluggish
- Fan noise increases

**Solutions:**

1. **Reduce Concurrent Connections:**
   ```json
   {
     "Application": {
       "Performance": {
         "MaxConcurrentConnections": 3  // Reduce from default 10
       }
     }
   }
   ```

2. **Disable Performance Monitoring:**
   - Close Health Metrics panel if not needed
   - Performance monitoring adds CPU overhead

3. **Check for Background Processes:**
   - Task Manager → Check for multiple instances
   - Close duplicate instances

4. **Update Graphics Drivers:**
   - Outdated drivers can cause high CPU usage
   - Update Windows and graphics drivers

---

### High Memory Usage

**Symptoms:**
- Application uses excessive RAM
- System memory warnings
- Application becomes unresponsive

**Solutions:**

1. **Limit Concurrent Sessions:**
   - Each session consumes memory
   - Close unused sessions

2. **Reduce Log Retention:**
   ```json
   {
     "Logging": {
       "File": {
         "RetentionDays": 7  // Reduce from 30
       }
     }
   }
   ```

3. **Clear Old Log Files:**
   ```powershell
   # Delete logs older than 7 days
   Get-ChildItem "$env:APPDATA\Aivana_RDP_WPF\Logs" | 
     Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | 
     Remove-Item
   ```

4. **Restart Application:**
   - Memory leaks may accumulate over time
   - Regular restarts help

---

## Database Issues

### "Database is Locked" Error

**Symptoms:**
- Cannot save connection profiles
- Database access errors
- "SQLite Error 5" messages

**Solutions:**

1. **Close Other Instances:**
   - Ensure only one instance is running
   - Check Task Manager for duplicate processes

2. **Check File Permissions:**
   - Verify write permissions on database file
   - Run application as Administrator if needed

3. **Wait and Retry:**
   - Database operations may be in progress
   - Wait a few seconds and retry

4. **Restart Application:**
   - Closes all database connections
   - Allows database to unlock

---

### Database Corruption

**Symptoms:**
- "Database disk image is malformed" error
- Connection profiles missing
- Application crashes when accessing data

**Solutions:**

1. **Backup Current Database:**
   ```powershell
   Copy-Item "$env:APPDATA\Aivana_RDP_WPF\aivana.db" "$env:APPDATA\Aivana_RDP_WPF\aivana.db.backup"
   ```

2. **Reset Database:**
   ```bash
   # Delete corrupted database
   Remove-Item "$env:APPDATA\Aivana_RDP_WPF\aivana.db"
   
   # Recreate
   cd Aivana_RDP_WPF
   dotnet ef database update
   ```
   **Warning:** All connection profiles will be lost!

3. **Restore from Backup:**
   - If you have a backup, restore it
   - Or re-import connections from exported files

---

## Credential Management Issues

### Passwords Not Saved

**Symptoms:**
- "Save Password" checked but password not saved
- Need to re-enter password each time

**Solutions:**

1. **Check Credential Manager:**
   - Open Windows Credential Manager
   - Look for entries starting with `Aivana_RDP_WPF_Profile_`
   - Verify credentials exist

2. **Re-save Credentials:**
   - Edit connection
   - Re-enter password
   - Check "Save Password"
   - Save connection

3. **Run as Administrator:**
   - Some systems require admin rights for Credential Manager
   - Right-click → Run as Administrator

---

### Cannot Delete Saved Credentials

**Symptoms:**
- Credentials persist after unchecking "Save Password"
- Cannot remove credentials

**Solutions:**

1. **Manual Deletion via Credential Manager:**
   - Open Windows Credential Manager
   - Find `Aivana_RDP_WPF_Profile_<ID>` entries
   - Delete manually

2. **Command Line:**
   ```powershell
   # List all Aivana credentials
   cmdkey /list | Select-String "Aivana"
   
   # Delete specific credential
   cmdkey /delete:Aivana_RDP_WPF_Profile_<ID>
   ```

3. **Re-create Connection:**
   - Delete connection profile
   - Create new connection
   - Don't check "Save Password"

---

## UI/Display Issues

### Theme Not Applying

**Symptoms:**
- Theme setting changes but UI doesn't update
- Stuck on Light or Dark theme

**Solutions:**

1. **Restart Application:**
   - Theme changes require application restart
   - Close and reopen application

2. **Check Settings File:**
   - Verify `appsettings.User.json` contains theme setting
   - Manually edit if needed:
   ```json
   {
     "Application": {
       "UI": {
         "Theme": "Dark"
       }
     }
   }
   ```

3. **Clear Resource Cache:**
   - Delete `bin` and `obj` folders
   - Rebuild solution

---

### RDP Session Not Displaying

**Symptoms:**
- Connection succeeds but remote desktop not visible
- Black screen in session area
- "RDP Session will appear here" message persists

**Solutions:**

1. **Check RDP Control Initialization:**
   - Verify `mstscax.dll` is available (Windows system file)
   - Check `AxMSTSCLib.dll` and `MSTSCLib.dll` are referenced

2. **Reconnect:**
   - Disconnect and reconnect
   - Sometimes RDP control needs reinitialization

3. **Check Display Settings:**
   - Verify resolution settings are valid
   - Try different resolution

4. **Restart Application:**
   - RDP ActiveX control may need reset

---

## File Transfer Issues

### File Transfer Not Working

**Symptoms:**
- Transfer starts but doesn't complete
- "Transfer failed" error
- No files transferred

**Solutions:**

1. **Check RDP Virtual Channels:**
   - File transfer requires RDP Virtual Channels support
   - Verify remote server supports file transfer
   - Test with built-in Windows RDP client

2. **Check Permissions:**
   - Verify write permissions on destination
   - Check disk space on both local and remote

3. **Network Issues:**
   - File transfer requires stable connection
   - Check network stability
   - Retry transfer

**Note:** File Transfer currently uses simulation mode. Full RDP Virtual Channels integration is planned.

---

## Clipboard Sync Issues

### Clipboard Not Syncing

**Symptoms:**
- Copy on local doesn't appear in remote session
- Copy in remote doesn't appear locally

**Solutions:**

1. **Check Clipboard Sync Enabled:**
   - Connection settings → Verify "Enable Clipboard Sync" is checked
   - Re-enable if disabled

2. **Restart Clipboard Service:**
   - Disconnect and reconnect
   - Clipboard service initializes on connection

3. **Check Clipboard Format:**
   - Some formats may not be supported
   - Try plain text first
   - Images and files may have limited support

**Note:** Clipboard synchronization currently supports basic text and image formats. Full format preservation is in development.

---

## Log File Analysis

### Log File Location

Log files are located at:
`%AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log`

Example: `C:\Users\John\AppData\Roaming\Aivana_RDP_WPF\Logs\aivana-2025-11-27.log`

### Reading Log Files

**Open in Text Editor:**
- Notepad, VS Code, or any text editor
- Logs are plain text with timestamps

**Log Format:**
```
2025-11-27 10:15:23.456 [Information] Connecting to 192.168.1.100:3389
2025-11-27 10:15:24.123 [Error] Connection failed: Timeout
```

### Common Log Messages

**Connection Issues:**
- `Connection failed: Timeout` - Server not responding
- `Connection failed: Access denied` - Credentials incorrect
- `RDP client error: {ErrorCode}` - RDP-specific error

**Database Issues:**
- `SQLite Error 14: unable to open database file` - File permissions
- `SQLite Error 5: database is locked` - Concurrent access

**Performance:**
- `High CPU usage detected` - Performance warning
- `Memory usage: {MB}MB` - Memory monitoring

---

## Known Issues

### Issue: File Transfer Simulation Mode

**Status:** Known Limitation  
**Description:** File Transfer currently simulates transfers instead of using real RDP Virtual Channels.

**Workaround:** Use Windows built-in RDP file transfer or wait for full implementation.

---

### Issue: Session Recording Placeholder Files

**Status:** Known Limitation  
**Description:** Session Recording creates placeholder files instead of actual video recordings.

**Workaround:** Use third-party screen recording tools or wait for Windows Media Foundation integration.

---

### Issue: Clipboard Format Limitations

**Status:** Known Limitation  
**Description:** Clipboard sync supports basic text and images. Complex formats may not sync correctly.

**Workaround:** Use plain text for critical clipboard operations.

---

### Issue: Performance Monitoring Local Metrics Only

**Status:** Known Limitation  
**Description:** Performance monitoring shows local CPU/Memory, not remote server metrics.

**Workaround:** Use remote server's built-in performance monitoring tools.

---

## Getting Additional Help

### Check Documentation

- **[User Guide](USER_GUIDE.md)** - Complete user documentation
- **[Developer Guide](DEVELOPER_GUIDE.md)** - Development troubleshooting
- **[Configuration Guide](CONFIGURATION_GUIDE.md)** - Configuration options

### Report Issues

1. **Check Existing Issues:**
   - Search [GitHub Issues](https://github.com/yourusername/Aivana-RDP-WPF/issues)
   - Check if issue already reported

2. **Create New Issue:**
   - Include error messages
   - Attach relevant log files (remove sensitive information)
   - Describe steps to reproduce
   - Include system information (Windows version, .NET version)

3. **Provide Information:**
   - Error messages (exact text)
   - Log file excerpts
   - Steps to reproduce
   - Expected vs. actual behavior

---

**Last Updated:** 2025-11-27

