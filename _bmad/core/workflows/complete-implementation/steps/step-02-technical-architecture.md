# Step 2: Phase 1 Technical Architecture Design

## MANDATORY EXECUTION RULES (READ FIRST):

- 🛑 NEVER implement without user confirmation on architectural decisions
- ✅ ALWAYS build upon existing AIVANA RDP WPF foundation
- 📋 YOU ARE TECHNICAL ARCHITECT designing scalable, maintainable systems
- 💬 FOCUS on practical architecture that can be implemented immediately
- 🚪 LEVERAGE existing codebase patterns and infrastructure
- ✅ YOU MUST ALWAYS SPEAK OUTPUT In your Agent communication style with the `communication_language`

## EXECUTION PROTOCOLS:

- 🎯 Show architectural analysis before implementation
- 💾 Create detailed technical specifications
- 📖 Build upon existing MVVM + DI + EF Core patterns
- 🚫 FORBIDDEN breaking changes without explicit user approval

## CONTEXT BOUNDARIES:

- Existing AIVANA RDP WPF codebase analyzed
- Current architecture: MVVM + DI + SQLite + WPF
- Phase 1 features: 8 specific deliverables identified
- 3-month timeline with 3-4 developer team

## YOUR TASK:

Design detailed technical architecture for Phase 1 implementation, building upon existing foundation while adding multi-protocol support and new features.

## ARCHITECTURE ANALYSIS:

### Current Foundation Assessment
✅ **Existing Patterns:**
- MVVM with CommunityToolkit.Mvvm
- Dependency Injection with Microsoft.Extensions.DependencyInjection
- Entity Framework Core with SQLite
- WPF with Fluent Design
- Service Layer Pattern

✅ **Existing Services:**
- ConnectionProfileService
- CredentialService (Windows Credential Manager)
- RdpConnectionService
- NotificationService
- ThemeService
- ImportExportService

### Phase 1 Architecture Requirements
🔴 **Multi-Protocol Support:**
- Abstract protocol interface
- RDP implementation (existing)
- SSH implementation (new)
- VNC implementation (new)

🔴 **Performance Monitoring:**
- Real metrics collection
- Network diagnostics
- Session health tracking

🔴 **Enhanced UI Components:**
- Quick-Connect Bar
- Settings UI expansion
- Health Dashboard

## TECHNICAL ARCHITECTURE DESIGN:

### 1. Multi-Protocol Architecture

```csharp
// Core Protocol Interface
public interface IRemoteProtocol {
    string ProtocolName { get; }
    Task<ConnectionResult> ConnectAsync(ConnectionProfile profile, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    Task<ProtocolCapabilities> GetCapabilitiesAsync();
    Task<PerformanceMetrics> GetMetricsAsync();
    event EventHandler<PerformanceMetrics> MetricsUpdated;
}

// Protocol Capabilities
public class ProtocolCapabilities {
    public bool SupportsFileTransfer { get; set; }
    public bool SupportsClipboardSync { get; set; }
    public bool SupportsPrinterRedirection { get; set; }
    public bool SupportsAudioRedirection { get; set; }
    public int MaxColorDepth { get; set; }
    public bool SupportsMultiMonitor { get; set; }
}

// Enhanced Connection Profile
public class ConnectionProfile {
    // Existing properties...
    public ProtocolType ProtocolType { get; set; } = ProtocolType.RDP;
    public string ProtocolSpecificSettings { get; set; } // JSON for protocol-specific config
}

// Protocol Factory
public interface IProtocolFactory {
    IRemoteProtocol CreateProtocol(ProtocolType type);
    IEnumerable<ProtocolType> GetSupportedProtocols();
}
```

### 2. Performance Monitoring Infrastructure

```csharp
// Real Performance Metrics (not simulated)
public class PerformanceMetrics {
    public DateTime Timestamp { get; set; }
    public int ConnectionProfileId { get; set; }
    
    // Network Metrics
    public double LatencyMs { get; set; }
    public double BandwidthMbps { get; set; }
    public double PacketLossPercent { get; set; }
    
    // Session Metrics
    public double FrameRate { get; set; }
    public int ColorDepth { get; set; }
    public int ResolutionWidth { get; set; }
    public int ResolutionHeight { get; set; }
    
    // System Metrics
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    
    // Quality Score
    public int QualityScore { get; set; } // 0-100
    public ConnectionQuality Quality { get; set; }
}

// Performance Monitoring Service
public interface IPerformanceMonitorService {
    Task StartMonitoringAsync(int connectionProfileId, CancellationToken ct = default);
    Task StopMonitoringAsync(int connectionProfileId, CancellationToken ct = default);
    Task<PerformanceMetrics?> GetCurrentMetricsAsync(int connectionProfileId, CancellationToken ct = default);
    event EventHandler<PerformanceMetrics> MetricsUpdated;
}
```

### 3. Quick-Connect Architecture

```csharp
// Quick-Connect Service
public interface IQuickConnectService {
    Task<List<ConnectionSuggestion>> GetSuggestionsAsync(string query, CancellationToken ct = default);
    Task<ConnectionProfile> CreateProfileFromQuickConnectAsync(string connectionString, CancellationToken ct = default);
    Task<ConnectionResult> QuickConnectAsync(string connectionString, CancellationToken ct = default);
}

// Connection Suggestions
public class ConnectionSuggestion {
    public ConnectionProfile Profile { get; set; }
    public string MatchedText { get; set; }
    public SuggestionType Type { get; set; }
    public DateTime LastConnected { get; set; }
    public int ConnectionCount { get; set; }
}

// Quick-Connect ViewModel
public class QuickConnectViewModel : ObservableObject {
    [ObservableProperty]
    private string _connectionString = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<ConnectionSuggestion> _suggestions = new();
    
    [ObservableProperty]
    private bool _isConnecting;
    
    public IAsyncRelayCommand ConnectCommand { get; }
    public IAsyncRelayCommand<string> SuggestionSelectedCommand { get; }
}
```

### 4. Wake-on-LAN Implementation

```csharp
// Wake-on-LAN Service
public interface IWakeOnLanService {
    Task<WakeResult> SendWakePacketAsync(string macAddress, string broadcastAddress, CancellationToken ct = default);
    Task<bool> WaitForHostAsync(string hostAddress, TimeSpan timeout, CancellationToken ct = default);
    Task<WakeResult> WakeConnectionAsync(ConnectionProfile profile, CancellationToken ct = default);
}

// Wake Result
public class WakeResult {
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; }
    public string MacAddress { get; set; }
    public string BroadcastAddress { get; set; }
}

// Enhanced Connection Profile with WoL
public class ConnectionProfile {
    // Existing properties...
    public string MacAddress { get; set; }
    public bool EnableWakeOnLan { get; set; }
    public int WakeTimeoutSeconds { get; set; } = 30;
}
```

### 5. SSH Tunnel Architecture

```csharp
// SSH Tunnel Service
public interface ISshTunnelService {
    Task<TunnelResult> CreateTunnelAsync(SshTunnelConfiguration config, CancellationToken ct = default);
    Task CloseTunnelAsync(string tunnelId, CancellationToken ct = default);
    Task<bool> IsTunnelActiveAsync(string tunnelId, CancellationToken ct = default);
}

// SSH Tunnel Configuration
public class SshTunnelConfiguration {
    public string SshHost { get; set; }
    public int SshPort { get; set; } = 22;
    public string SshUser { get; set; }
    public string PrivateKeyPath { get; set; }
    public string Password { get; set; }
    public string LocalHost { get; set; } = "localhost";
    public int LocalPort { get; set; }
    public string RemoteHost { get; set; }
    public int RemotePort { get; set; }
}

// Enhanced Connection Profile with SSH
public class ConnectionProfile {
    // Existing properties...
    public bool UseSshTunnel { get; set; }
    public SshTunnelConfiguration SshTunnelConfig { get; set; }
}
```

### 6. Enhanced Settings Architecture

```csharp
// Settings Categories
public enum SettingsCategory {
    General,
    Connection,
    Security,
    Performance,
    UserInterface,
    Advanced
}

// Settings Service
public interface ISettingsService {
    Task<T> GetSettingAsync<T>(string key, T defaultValue = default);
    Task SetSettingAsync<T>(string key, T value);
    Task ResetToDefaultsAsync();
    Task ExportSettingsAsync(string filePath);
    Task ImportSettingsAsync(string filePath);
}

// Settings ViewModels
public class GeneralSettingsViewModel : ObservableObject {
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private bool _minimizeToTray;
    [ObservableProperty] private bool _checkForUpdates;
    [ObservableProperty] private string _defaultConnectionFolder;
}

public class ConnectionSettingsViewModel : ObservableObject {
    [ObservableProperty] private bool _autoReconnect;
    [ObservableProperty] private int _reconnectAttempts;
    [ObservableProperty] private TimeSpan _reconnectDelay;
    [ObservableProperty] private bool _showConnectionThumbnails;
}
```

### 7. Dependency Injection Configuration

```csharp
// Enhanced Service Registration
public static class ServiceCollectionExtensions {
    public static IServiceCollection AddAivanaServices(this IServiceCollection services) {
        // Existing services...
        services.AddSingleton<IConnectionProfileService, ConnectionProfileService>();
        services.AddSingleton<ICredentialService, CredentialService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        
        // New Phase 1 services...
        services.AddSingleton<IProtocolFactory, ProtocolFactory>();
        services.AddTransient<IRemoteProtocol, RdpProtocol>();
        services.AddTransient<IRemoteProtocol, SshProtocol>();
        services.AddTransient<IRemoteProtocol, VncProtocol>();
        
        services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();
        services.AddSingleton<IQuickConnectService, QuickConnectService>();
        services.AddSingleton<IWakeOnLanService, WakeOnLanService>();
        services.AddSingleton<ISshTunnelService, SshTunnelService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        
        return services;
    }
}
```

### 8. Database Schema Updates

```sql
-- Enhanced Connection Profiles
ALTER TABLE ConnectionProfiles ADD COLUMN ProtocolType INTEGER NOT NULL DEFAULT 0;
ALTER TABLE ConnectionProfiles ADD COLUMN ProtocolSpecificSettings TEXT;
ALTER TABLE ConnectionProfiles ADD COLUMN MacAddress TEXT;
ALTER TABLE ConnectionProfiles ADD COLUMN EnableWakeOnLan BOOLEAN NOT NULL DEFAULT 0;
ALTER TABLE ConnectionProfiles ADD COLUMN WakeTimeoutSeconds INTEGER NOT NULL DEFAULT 30;
ALTER TABLE ConnectionProfiles ADD COLUMN UseSshTunnel BOOLEAN NOT NULL DEFAULT 0;

-- New Settings Table
CREATE TABLE Settings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL UNIQUE,
    Value TEXT NOT NULL,
    Category TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

-- Performance Metrics History
CREATE TABLE PerformanceMetrics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConnectionProfileId INTEGER NOT NULL,
    Timestamp DATETIME NOT NULL,
    LatencyMs REAL,
    BandwidthMbps REAL,
    PacketLossPercent REAL,
    FrameRate REAL,
    CpuUsagePercent REAL,
    MemoryUsageMB REAL,
    QualityScore INTEGER,
    FOREIGN KEY (ConnectionProfileId) REFERENCES ConnectionProfiles(Id)
);
```

## IMPLEMENTATION SEQUENCE:

### Week 1-2: Foundation
1. Multi-Protocol Interface Design
2. Protocol Factory Implementation
3. Database Schema Updates
4. Basic SSH Protocol Stub

### Week 3-4: Core Features
1. Quick-Connect Service Implementation
2. Performance Monitoring Infrastructure
3. Enhanced Connection Profiles
4. Settings Service Foundation

### Week 5-6: Advanced Features
1. Wake-on-LAN Implementation
2. SSH Tunnel Implementation
3. VNC Protocol Support
4. Health Dashboard UI

### Week 7-8: Polish & Testing
1. UI/UX Refinements
2. Performance Optimization
3. Comprehensive Testing
4. Documentation Updates

## QUALITY REQUIREMENTS:

- **Test Coverage**: > 80% for new code
- **Performance**: < 2s startup, < 100ms UI response
- **Memory**: < 200MB for typical usage
- **Security**: No credential leaks, secure storage
- **Compatibility**: Windows 10+ with .NET 8+

## SUCCESS METRICS:

✅ Technical architecture designed and documented
✅ Implementation sequence planned with realistic timelines
✅ Quality requirements and success criteria defined
✅ Database schema and service contracts specified
✅ Dependency injection and patterns established

## NEXT STEPS:

Based on user approval of technical architecture, proceed to step-03-implementation-planning.md to create detailed task breakdown and sprint planning for Phase 1.

Remember: Build incrementally, test thoroughly, and maintain architectural consistency throughout implementation.
