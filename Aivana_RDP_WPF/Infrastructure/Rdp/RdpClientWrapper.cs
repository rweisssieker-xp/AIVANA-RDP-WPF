using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Extensions.Logging;
using AxMSTSCLib;
using MSTSCLib;

namespace Aivana_RDP_WPF.Infrastructure.Rdp;

/// <summary>
/// Wrapper for MSTSC ActiveX Control (RDP client).
/// </summary>
public class RdpClientWrapper : IDisposable
{
    private readonly ILogger<RdpClientWrapper> _logger;
    private WindowsFormsHost? _host;
    private Panel? _containerPanel;
    private AxMSTSCLib.AxMsRdpClient9NotSafeForScripting? _rdpClient;
    private bool _isConnected;
    private bool _smartSizingEnabled = true;
    private System.Windows.Forms.Timer? _resizeTimer;

    public RdpClientWrapper(ILogger<RdpClientWrapper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets whether smart sizing is enabled.
    /// When enabled, the RDP display scales to fit the window.
    /// </summary>
    public bool SmartSizingEnabled
    {
        get => _smartSizingEnabled;
        set
        {
            _smartSizingEnabled = value;
            ApplySmartSizing();
        }
    }

    /// <summary>
    /// Creates a WindowsFormsHost containing the RDP control.
    /// </summary>
    public WindowsFormsHost CreateHost()
    {
        try
        {
            _logger.LogInformation("Creating RDP client host");
            _host = new WindowsFormsHost();
            
            // Create a container panel first
            _containerPanel = new Panel();
            _containerPanel.Dock = DockStyle.Fill;
            _containerPanel.BackColor = System.Drawing.Color.Black;
            
            // Create MSTSC ActiveX Control
            _rdpClient = new AxMSTSCLib.AxMsRdpClient9NotSafeForScripting();
            
            // Add to panel first (this initializes the control)
            _containerPanel.Controls.Add(_rdpClient);
            
            // Configure RDP client settings - Dock.Fill handles all sizing
            _rdpClient.Size = new System.Drawing.Size(1024, 768);
            _rdpClient.Dock = DockStyle.Fill;
            // Note: Anchor is ignored when Dock is set to Fill
            
            // Force control creation
            _rdpClient.CreateControl();
            
            // Set up event handlers
            _rdpClient.OnConnected += RdpClient_OnConnected;
            _rdpClient.OnDisconnected += RdpClient_OnDisconnected;
            _rdpClient.OnLoginComplete += RdpClient_OnLoginComplete;
            _rdpClient.OnFatalError += RdpClient_OnFatalError;
            _rdpClient.OnWarning += RdpClient_OnWarning;
            
            // Handle resize events with debouncing
            _containerPanel.Resize += ContainerPanel_Resize;
            
            // Setup resize debounce timer
            _resizeTimer = new System.Windows.Forms.Timer();
            _resizeTimer.Interval = 250; // 250ms debounce
            _resizeTimer.Tick += ResizeTimer_Tick;
            
            // Add panel to host
            _host.Child = _containerPanel;
            
            _logger.LogInformation("RDP client host created successfully");
            return _host;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RDP client host");
            throw;
        }
    }

    private void ContainerPanel_Resize(object? sender, EventArgs e)
    {
        // Debounce resize events
        _resizeTimer?.Stop();
        _resizeTimer?.Start();
    }

    private void ResizeTimer_Tick(object? sender, EventArgs e)
    {
        _resizeTimer?.Stop();
        ApplySmartSizing();
    }

    private void ApplySmartSizing()
    {
        if (_rdpClient == null || !_rdpClient.IsHandleCreated)
            return;

        try
        {
            // Get the advanced settings interface for smart sizing
            var advSettings = _rdpClient.AdvancedSettings9;
            if (advSettings != null)
            {
                advSettings.SmartSizing = _smartSizingEnabled;
            }
            
            _logger.LogDebug("Smart sizing set to {Enabled}", _smartSizingEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply smart sizing");
        }
    }

    /// <summary>
    /// Updates the RDP session resolution dynamically (if supported).
    /// </summary>
    public void UpdateSessionResolution(int width, int height)
    {
        if (_rdpClient == null || !_isConnected)
            return;

        try
        {
            // Try to update resolution dynamically (RDP 8.1+)
            var rdpClient8 = (IMsRdpClient8)_rdpClient.GetOcx();
            rdpClient8.Reconnect((uint)width, (uint)height);
            _logger.LogInformation("Updated session resolution to {Width}x{Height}", width, height);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dynamic resolution update not supported, using smart sizing");
        }
    }

    /// <summary>
    /// Connects to the remote desktop.
    /// </summary>
    public void Connect(string server, int port, string username, string? domain = null, string? password = null)
    {
        if (_rdpClient == null)
            throw new InvalidOperationException("RDP client not initialized. Call CreateHost() first.");

        if (_isConnected)
        {
            _logger.LogWarning("Already connected. Disconnecting first.");
            Disconnect();
        }

        try
        {
            _logger.LogInformation("Connecting to {Server}:{Port} as {Username}", server, port, username);
            
            // Ensure control is created and ready
            if (!_rdpClient.IsHandleCreated)
            {
                _rdpClient.CreateControl();
                // Wait for control to be ready
                while (!_rdpClient.IsHandleCreated)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }
            }
            
            // Configure connection settings - must be done before Connect()
            _rdpClient.Server = server;
            _rdpClient.UserName = username ?? "";
            
            if (!string.IsNullOrEmpty(domain))
            {
                _rdpClient.Domain = domain;
            }
            
            // Set advanced settings via AdvancedSettings interface
            var advSettings = _rdpClient.AdvancedSettings9;
            advSettings.RDPPort = port;
            advSettings.Compress = 1; // Enable compression
            advSettings.BitmapPeristence = 1; // Enable bitmap caching
            advSettings.EnableAutoReconnect = true; // Enable auto-reconnect
            advSettings.AuthenticationLevel = 2; // Server authentication not required
            
            // Enable Smart Sizing for automatic scaling
            advSettings.SmartSizing = _smartSizingEnabled;
            
            // Enable display resolution updates (RDP 8+)
            advSettings.DisplayConnectionBar = true;
            
            // Set password if provided
            if (!string.IsNullOrEmpty(password))
            {
                advSettings.ClearTextPassword = password;
            }
            
            // Get container size for initial resolution
            int desktopWidth = 1920;
            int desktopHeight = 1080;
            
            if (_containerPanel != null && _containerPanel.Width > 0 && _containerPanel.Height > 0)
            {
                // Use container size or a reasonable default
                desktopWidth = Math.Max(_containerPanel.Width, 800);
                desktopHeight = Math.Max(_containerPanel.Height, 600);
                _logger.LogInformation("Using container size for resolution: {Width}x{Height}", desktopWidth, desktopHeight);
            }
            
            // Set display settings - use container dimensions
            _rdpClient.DesktopWidth = desktopWidth;
            _rdpClient.DesktopHeight = desktopHeight;
            _rdpClient.ColorDepth = 32;
            
            // Connect
            _rdpClient.Connect();
            
            _logger.LogInformation("RDP connection initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to RDP server {Server}:{Port}", server, port);
            throw;
        }
    }

    /// <summary>
    /// Disconnects from the remote desktop.
    /// </summary>
    public void Disconnect()
    {
        if (_rdpClient == null)
            return;

        try
        {
            // Check if we need to disconnect (control connected state, not just our flag)
            if (_isConnected || _rdpClient.Connected != 0)
            {
                _logger.LogInformation("Disconnecting from RDP session");
                _rdpClient.Disconnect();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from RDP session");
        }
        finally
        {
            _isConnected = false;
        }
    }

    public bool IsConnected => _isConnected;

    // Event handlers - use object? to fix nullable warnings
    private void RdpClient_OnConnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("RDP connection established");
        _isConnected = true;
        Connected?.Invoke(this, EventArgs.Empty);
    }

    private void RdpClient_OnDisconnected(object? sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
    {
        var reasonDescription = GetDisconnectReasonDescription(e.discReason);
        _logger.LogInformation("RDP connection disconnected. Reason: {Reason} ({ReasonCode})", reasonDescription, e.discReason);
        _isConnected = false;
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void RdpClient_OnLoginComplete(object? sender, EventArgs e)
    {
        _logger.LogInformation("RDP login completed");
        LoginComplete?.Invoke(this, EventArgs.Empty);
    }

    private void RdpClient_OnFatalError(object? sender, AxMSTSCLib.IMsTscAxEvents_OnFatalErrorEvent e)
    {
        var errorDescription = GetErrorCodeDescription(e.errorCode);
        _logger.LogError("RDP fatal error: {ErrorDescription} ({ErrorCode})", errorDescription, e.errorCode);
        FatalError?.Invoke(this, new RdpErrorEventArgs(e.errorCode));
    }

    private void RdpClient_OnWarning(object? sender, AxMSTSCLib.IMsTscAxEvents_OnWarningEvent e)
    {
        _logger.LogWarning("RDP warning: {WarningCode}", e.warningCode);
        Warning?.Invoke(this, new RdpWarningEventArgs(e.warningCode));
    }

    /// <summary>
    /// Gets human-readable description for disconnect reason codes.
    /// </summary>
    private static string GetDisconnectReasonDescription(int reasonCode)
    {
        return reasonCode switch
        {
            0 => "No error",
            1 => "Remote disconnection by user",
            2 => "Remote disconnection by server",
            3 => "DNS lookup failed",
            260 => "DNS lookup failed",
            262 => "Out of memory",
            264 => "Connection timed out",
            516 => "Socket connect failed",
            518 => "Out of memory",
            520 => "Host not found",
            772 => "Windows Sockets send call failed",
            774 => "Out of memory",
            776 => "Invalid IP address",
            1028 => "Windows Sockets recv call failed",
            1030 => "Invalid security data",
            1032 => "Internal error",
            1286 => "Invalid encryption method",
            1288 => "DNS lookup failed",
            1540 => "Windows Sockets gethostbyname call failed",
            1542 => "Invalid server security data",
            1544 => "Internal timer error",
            1796 => "Time-out occurred",
            1798 => "Failed to unpack server certificate",
            2052 => "Bad IP address specified",
            2056 => "License negotiation failed",
            2310 => "Internal security error",
            2312 => "Licensing timeout",
            2566 => "Internal security error",
            2822 => "Encryption error",
            2823 => "Decryption error",
            3078 => "Decompression error",
            _ => $"Unknown error code {reasonCode}"
        };
    }

    /// <summary>
    /// Gets human-readable description for error codes.
    /// </summary>
    private static string GetErrorCodeDescription(int errorCode)
    {
        return errorCode switch
        {
            0 => "Unknown error",
            1 => "Internal error",
            2 => "Out of memory",
            3 => "Window creation error",
            4 => "Internal error 2",
            5 => "Internal error 3",
            6 => "Internal error 4",
            7 => "Unrecoverable error",
            100 => "Winsock initialization error",
            _ => $"Error code {errorCode}"
        };
    }

    // Events
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler? LoginComplete;
    public event EventHandler<RdpErrorEventArgs>? FatalError;
    public event EventHandler<RdpWarningEventArgs>? Warning;

    /// <summary>
    /// Sets the RDP session to full screen mode.
    /// </summary>
    public void SetFullScreen(bool fullScreen)
    {
        if (_rdpClient == null)
            return;

        try
        {
            _rdpClient.FullScreen = fullScreen;
            _logger.LogInformation("Full screen mode set to {FullScreen}", fullScreen);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set full screen mode");
        }
    }

    /// <summary>
    /// Refreshes the display scaling.
    /// </summary>
    public void RefreshScaling()
    {
        ApplySmartSizing();
    }

    public void Dispose()
    {
        _resizeTimer?.Stop();
        _resizeTimer?.Dispose();
        
        if (_containerPanel != null)
        {
            _containerPanel.Resize -= ContainerPanel_Resize;
        }
        
        Disconnect();
        _rdpClient?.Dispose();
        _containerPanel?.Dispose();
        _host?.Dispose();
    }
}

/// <summary>
/// Event args for RDP errors.
/// </summary>
public class RdpErrorEventArgs : EventArgs
{
    public int ErrorCode { get; }

    public RdpErrorEventArgs(int errorCode)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Event args for RDP warnings.
/// </summary>
public class RdpWarningEventArgs : EventArgs
{
    public int WarningCode { get; }

    public RdpWarningEventArgs(int warningCode)
    {
        WarningCode = warningCode;
    }
}

