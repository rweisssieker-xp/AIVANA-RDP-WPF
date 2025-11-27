using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Extensions.Logging;
using AxMSTSCLib;

namespace Aivana_RDP_WPF.Infrastructure.Rdp;

/// <summary>
/// Wrapper for MSTSC ActiveX Control (RDP client).
/// </summary>
public class RdpClientWrapper : IDisposable
{
    private readonly ILogger<RdpClientWrapper> _logger;
    private WindowsFormsHost? _host;
    private AxMSTSCLib.AxMsRdpClient9NotSafeForScripting? _rdpClient;
    private bool _isConnected;

    public RdpClientWrapper(ILogger<RdpClientWrapper> logger)
    {
        _logger = logger;
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
            
            // Create MSTSC ActiveX Control
            _rdpClient = new AxMSTSCLib.AxMsRdpClient9NotSafeForScripting();
            
            // Configure RDP client settings
            _rdpClient.Size = new System.Drawing.Size(1024, 768);
            _rdpClient.Dock = DockStyle.Fill;
            
            // Set up event handlers
            _rdpClient.OnConnected += RdpClient_OnConnected;
            _rdpClient.OnDisconnected += RdpClient_OnDisconnected;
            _rdpClient.OnLoginComplete += RdpClient_OnLoginComplete;
            _rdpClient.OnFatalError += RdpClient_OnFatalError;
            _rdpClient.OnWarning += RdpClient_OnWarning;
            
            // Add control to host
            _host.Child = _rdpClient;
            
            _logger.LogInformation("RDP client host created successfully");
            return _host;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RDP client host");
            throw;
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
            
            // Configure connection settings
            _rdpClient.Server = server;
            _rdpClient.AdvancedSettings9.RDPPort = port;
            _rdpClient.UserName = username;
            
            if (!string.IsNullOrEmpty(domain))
            {
                _rdpClient.Domain = domain;
            }
            
            // Set password if provided
            if (!string.IsNullOrEmpty(password))
            {
                _rdpClient.AdvancedSettings9.ClearTextPassword = password;
            }

            // Set advanced settings
            _rdpClient.AdvancedSettings9.Compress = 1; // Enable compression
            _rdpClient.AdvancedSettings9.BitmapPeristence = 1; // Enable bitmap caching
            _rdpClient.AdvancedSettings9.EnableAutoReconnect = true; // Enable auto-reconnect
            
            // Set display settings
            _rdpClient.DesktopWidth = 1920;
            _rdpClient.DesktopHeight = 1080;
            _rdpClient.ColorDepth = 32;
            
            // Set security settings
            _rdpClient.AdvancedSettings9.AuthenticationLevel = 2; // No authentication required (can be changed)
            
            // If password provided, set it
            if (!string.IsNullOrEmpty(password))
            {
                _rdpClient.AdvancedSettings9.ClearTextPassword = password;
            }
            
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
        if (_rdpClient != null && _isConnected)
        {
            try
            {
                _logger.LogInformation("Disconnecting from RDP session");
                _rdpClient.Disconnect();
                _isConnected = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from RDP session");
            }
        }
    }

    public bool IsConnected => _isConnected;

    // Event handlers
    private void RdpClient_OnConnected(object sender, EventArgs e)
    {
        _logger.LogInformation("RDP connection established");
        _isConnected = true;
        Connected?.Invoke(this, EventArgs.Empty);
    }

    private void RdpClient_OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
    {
        _logger.LogInformation("RDP connection disconnected. Reason: {Reason}", e.discReason);
        _isConnected = false;
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void RdpClient_OnLoginComplete(object sender, EventArgs e)
    {
        _logger.LogInformation("RDP login completed");
        LoginComplete?.Invoke(this, EventArgs.Empty);
    }

    private void RdpClient_OnFatalError(object sender, AxMSTSCLib.IMsTscAxEvents_OnFatalErrorEvent e)
    {
        _logger.LogError("RDP fatal error: {ErrorCode}", e.errorCode);
        FatalError?.Invoke(this, new RdpErrorEventArgs(e.errorCode));
    }

    private void RdpClient_OnWarning(object sender, AxMSTSCLib.IMsTscAxEvents_OnWarningEvent e)
    {
        _logger.LogWarning("RDP warning: {WarningCode}", e.warningCode);
        Warning?.Invoke(this, new RdpWarningEventArgs(e.warningCode));
    }

    // Events
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler? LoginComplete;
    public event EventHandler<RdpErrorEventArgs>? FatalError;
    public event EventHandler<RdpWarningEventArgs>? Warning;

    public void Dispose()
    {
        Disconnect();
        _rdpClient?.Dispose();
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

