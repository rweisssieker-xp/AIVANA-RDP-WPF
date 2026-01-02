# Step 3: Week 1-2 Implementation Planning

## MANDATORY EXECUTION RULES (READ FIRST):

- 🛑 NEVER write code without confirming architectural decisions
- ✅ ALWAYS implement incrementally with testing
- 📋 YOU ARE IMPLEMENTATION LEAD coordinating development tasks
- 💬 FOCUS on practical, testable code that builds incrementally
- 🚪 START with foundation interfaces and build up from there
- ✅ YOU MUST ALWAYS SPEAK OUTPUT In your Agent communication style with the `communication_language`

## EXECUTION PROTOCOLS:

- 🎯 Show implementation plan before coding
- 💾 Create detailed task breakdown with file locations
- 📖 Build upon existing AIVANA RDP WPF project structure
- 🚫 FORBIDDEN breaking existing functionality

## CONTEXT BOUNDARIES:

- Working with existing AIVANA_RDP_WPF solution
- Current project structure: Models/, ViewModels/, Views/, Services/, Infrastructure/
- Week 1-2 focus: Multi-Protocol Interface + Database Updates
- 2 developers, 2 weeks, foundation tasks only

## WEEK 1-2 IMPLEMENTATION PLAN:

### Day 1-2: Multi-Protocol Interface Design

#### Task 1.1: Create Protocol Interfaces
**Files to Create/Modify:**
- `Aivana_RDP_WPF/Infrastructure/Protocols/IRemoteProtocol.cs`
- `Aivana_RDP_WPF/Infrastructure/Protocols/ProtocolCapabilities.cs`
- `Aivana_RDP_WPF/Infrastructure/Protocols/ProtocolType.cs`
- `Aivana_RDP_WPF/Infrastructure/Protocols/ConnectionResult.cs`

#### Task 1.2: Implement Protocol Factory
**Files to Create/Modify:**
- `Aivana_RDP_WPF/Infrastructure/Protocols/IProtocolFactory.cs`
- `Aivana_RDP_WPF/Infrastructure/Protocols/ProtocolFactory.cs`

#### Task 1.3: Update Existing RDP Implementation
**Files to Modify:**
- `Aivana_RDP_WPF/Infrastructure/Protocols/RdpProtocol.cs` (rename existing)
- `Aivana_RDP_WPF/Infrastructure/RdpConnectionService.cs` (update to use new interface)

### Day 3-4: Database Schema Updates

#### Task 2.1: Create Database Migration
**Files to Create/Modify:**
- `Aivana_RDP_WPF/Data/Migrations/20260102_AddMultiProtocolSupport.cs`
- `Aivana_RDP_WPF/Models/ProtocolType.cs`
- `Aivana_RDP_WPF/Models/PerformanceMetrics.cs`

#### Task 2.2: Update Models
**Files to Modify:**
- `Aivana_RDP_WPF/Models/ConnectionProfile.cs` (add new properties)

### Day 5-6: SSH Protocol Stub

#### Task 3.1: Create SSH Protocol Implementation
**Files to Create/Modify:**
- `Aivana_RDP_WPF/Infrastructure/Protocols/SshProtocol.cs`
- `Aivana_RDP_WPF/Infrastructure/Protocols/VncProtocol.cs` (stub)

#### Task 3.2: Update Dependency Injection
**Files to Modify:**
- `Aivana_RDP_WPF/App.xaml.cs` (service registration)

### Day 7-8: Testing & Validation

#### Task 4.1: Create Unit Tests
**Files to Create/Modify:**
- `Aivana_RDP_WPF.Tests/Infrastructure/Protocols/ProtocolFactoryTests.cs`
- `Aivana_RDP_WPF.Tests/Infrastructure/Protocols/RdpProtocolTests.cs`

#### Task 4.2: Integration Testing
**Files to Create/Modify:**
- `Aivana_RDP_WPF.Tests/Integration/MultiProtocolIntegrationTests.cs`

## DETAILED IMPLEMENTATION TASKS:

### Task 1.1: Protocol Interfaces

```csharp
// File: Aivana_RDP_WPF/Infrastructure/Protocols/ProtocolType.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public enum ProtocolType {
    RDP = 0,
    SSH = 1,
    VNC = 2
}

// File: Aivana_RDP_WPF/Infrastructure/Protocols/ProtocolCapabilities.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public class ProtocolCapabilities {
    public bool SupportsFileTransfer { get; set; }
    public bool SupportsClipboardSync { get; set; }
    public bool SupportsPrinterRedirection { get; set; }
    public bool SupportsAudioRedirection { get; set; }
    public int MaxColorDepth { get; set; }
    public bool SupportsMultiMonitor { get; set; }
    public List<string> SupportedAuthMethods { get; set; } = new();
}

// File: Aivana_RDP_WPF/Infrastructure/Protocols/ConnectionResult.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public class ConnectionResult {
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTime ConnectedAt { get; set; }
}

// File: Aivana_RDP_WPF/Infrastructure/Protocols/IRemoteProtocol.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public interface IRemoteProtocol {
    ProtocolType ProtocolType { get; }
    string ProtocolName { get; }
    
    Task<ConnectionResult> ConnectAsync(ConnectionProfile profile, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    Task<ProtocolCapabilities> GetCapabilitiesAsync();
    Task<PerformanceMetrics> GetMetricsAsync();
    
    event EventHandler<PerformanceMetrics>? MetricsUpdated;
    event EventHandler<string>? StatusChanged;
}
```

### Task 1.2: Protocol Factory

```csharp
// File: Aivana_RDP_WPF/Infrastructure/Protocols/IProtocolFactory.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public interface IProtocolFactory {
    IRemoteProtocol CreateProtocol(ProtocolType type);
    IEnumerable<ProtocolType> GetSupportedProtocols();
    ProtocolCapabilities GetCapabilities(ProtocolType type);
}

// File: Aivana_RDP_WPF/Infrastructure/Protocols/ProtocolFactory.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public class ProtocolFactory : IProtocolFactory {
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ProtocolType, Type> _protocolTypes = new();

    public ProtocolFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
        _protocolTypes[ProtocolType.RDP] = typeof(RdpProtocol);
        _protocolTypes[ProtocolType.SSH] = typeof(SshProtocol);
        _protocolTypes[ProtocolType.VNC] = typeof(VncProtocol);
    }

    public IRemoteProtocol CreateProtocol(ProtocolType type) {
        if (_protocolTypes.TryGetValue(type, out var protocolType)) {
            return (IRemoteProtocol)_serviceProvider.GetRequiredService(protocolType);
        }
        throw new NotSupportedException($"Protocol type {type} is not supported");
    }

    public IEnumerable<ProtocolType> GetSupportedProtocols() {
        return _protocolTypes.Keys;
    }

    public ProtocolCapabilities GetCapabilities(ProtocolType type) {
        var protocol = CreateProtocol(type);
        return protocol.GetCapabilitiesAsync().GetAwaiter().GetResult();
    }
}
```

### Task 2.1: Database Migration

```csharp
// File: Aivana_RDP_WPF/Data/Migrations/20260102_AddMultiProtocolSupport.cs
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aivana_RDP_WPF.Data.Migrations;

public partial class AddMultiProtocolSupport : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.AddColumn<int>(
            name: "ProtocolType",
            table: "ConnectionProfiles",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "ProtocolSpecificSettings",
            table: "ConnectionProfiles",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MacAddress",
            table: "ConnectionProfiles",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "EnableWakeOnLan",
            table: "ConnectionProfiles",
            type: "BOOLEAN",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "WakeTimeoutSeconds",
            table: "ConnectionProfiles",
            type: "INTEGER",
            nullable: false,
            defaultValue: 30);

        migrationBuilder.AddColumn<bool>(
            name: "UseSshTunnel",
            table: "ConnectionProfiles",
            type: "BOOLEAN",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "PerformanceMetrics",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ConnectionProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                LatencyMs = table.Column<double>(type: "REAL", nullable: true),
                BandwidthMbps = table.Column<double>(type: "REAL", nullable: true),
                PacketLossPercent = table.Column<double>(type: "REAL", nullable: true),
                FrameRate = table.Column<double>(type: "REAL", nullable: true),
                CpuUsagePercent = table.Column<double>(type: "REAL", nullable: true),
                MemoryUsageMB = table.Column<double>(type: "REAL", nullable: true),
                QualityScore = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table => {
                table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                table.ForeignKey(
                    name: "FK_PerformanceMetrics_ConnectionProfiles_ConnectionProfileId",
                    column: x => x.ConnectionProfileId,
                    principalTable: "ConnectionProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Key = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: false),
                Category = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => {
                table.PrimaryKey("PK_Settings", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PerformanceMetrics_ConnectionProfileId",
            table: "PerformanceMetrics",
            column: "ConnectionProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_Settings_Key",
            table: "Settings",
            column: "Key",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable(
            name: "PerformanceMetrics");

        migrationBuilder.DropTable(
            name: "Settings");

        migrationBuilder.DropColumn(
            name: "ProtocolType",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "ProtocolSpecificSettings",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "MacAddress",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "EnableWakeOnLan",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "WakeTimeoutSeconds",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "UseSshTunnel",
            table: "ConnectionProfiles");
    }
}
```

### Task 2.2: Updated Connection Profile

```csharp
// File: Aivana_RDP_WPF/Models/ConnectionProfile.cs (add to existing)
using Aivana_RDP_WPF.Infrastructure.Protocols;

public partial class ConnectionProfile {
    // Existing properties...
    
    // New multi-protocol properties
    public ProtocolType ProtocolType { get; set; } = ProtocolType.RDP;
    public string? ProtocolSpecificSettings { get; set; }
    
    // Wake-on-LAN properties
    public string? MacAddress { get; set; }
    public bool EnableWakeOnLan { get; set; }
    public int WakeTimeoutSeconds { get; set; } = 30;
    
    // SSH Tunnel properties
    public bool UseSshTunnel { get; set; }
    public string? SshTunnelConfig { get; set; } // JSON serialized
}
```

### Task 3.1: SSH Protocol Stub

```csharp
// File: Aivana_RDP_WPF/Infrastructure/Protocols/SshProtocol.cs
namespace Aivana_RDP_WPF.Infrastructure.Protocols;

public class SshProtocol : IRemoteProtocol {
    private readonly ILogger<SshProtocol> _logger;
    
    public ProtocolType ProtocolType => ProtocolType.SSH;
    public string ProtocolName => "SSH";
    
    public event EventHandler<PerformanceMetrics>? MetricsUpdated;
    public event EventHandler<string>? StatusChanged;

    public SshProtocol(ILogger<SshProtocol> logger) {
        _logger = logger;
    }

    public async Task<ConnectionResult> ConnectAsync(ConnectionProfile profile, CancellationToken ct = default) {
        StatusChanged?.Invoke(this, "Connecting to SSH server...");
        
        try {
            // TODO: Implement actual SSH connection
            await Task.Delay(1000, ct); // Simulate connection
            
            return new ConnectionResult {
                Success = true,
                Message = "SSH connection established",
                SessionId = $"ssh_{Guid.NewGuid()}",
                ConnectedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "SSH connection failed");
            return new ConnectionResult {
                Success = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default) {
        StatusChanged?.Invoke(this, "Disconnecting SSH session...");
        // TODO: Implement actual SSH disconnection
        await Task.Delay(500, ct);
    }

    public async Task<ProtocolCapabilities> GetCapabilitiesAsync() {
        return await Task.FromResult(new ProtocolCapabilities {
            SupportsFileTransfer = true,
            SupportsClipboardSync = false,
            SupportsPrinterRedirection = false,
            SupportsAudioRedirection = false,
            MaxColorDepth = 24,
            SupportsMultiMonitor = false,
            SupportedAuthMethods = new() { "password", "publickey" }
        });
    }

    public async Task<PerformanceMetrics> GetMetricsAsync() {
        // TODO: Implement actual SSH metrics collection
        return await Task.FromResult(new PerformanceMetrics {
            Timestamp = DateTime.UtcNow,
            LatencyMs = 25.0,
            BandwidthMbps = 50.0,
            QualityScore = 85
        });
    }
}
```

### Task 4.1: Unit Tests

```csharp
// File: Aivana_RDP_WPF.Tests/Infrastructure/Protocols/ProtocolFactoryTests.cs
namespace Aivana_RDP_WPF.Tests.Infrastructure.Protocols;

[TestFixture]
public class ProtocolFactoryTests {
    private ServiceProvider _serviceProvider;
    private IProtocolFactory _factory;

    [SetUp]
    public void Setup() {
        var services = new ServiceCollection();
        services.AddSingleton<IProtocolFactory, ProtocolFactory>();
        services.AddTransient<RdpProtocol>();
        services.AddTransient<SshProtocol>();
        services.AddTransient<VncProtocol>();
        
        _serviceProvider = services.BuildServiceProvider();
        _factory = _serviceProvider.GetRequiredService<IProtocolFactory>();
    }

    [Test]
    public void CreateProtocol_WithValidType_ReturnsCorrectProtocol() {
        // Arrange
        var expectedType = typeof(SshProtocol);
        
        // Act
        var protocol = _factory.CreateProtocol(ProtocolType.SSH);
        
        // Assert
        Assert.IsInstanceOf<SshProtocol>(protocol);
        Assert.AreEqual(ProtocolType.SSH, protocol.ProtocolType);
    }

    [Test]
    public void GetSupportedProtocols_ReturnsAllSupportedTypes() {
        // Act
        var protocols = _factory.GetSupportedProtocols();
        
        // Assert
        Assert.Contains(ProtocolType.RDP, protocols);
        Assert.Contains(ProtocolType.SSH, protocols);
        Assert.Contains(ProtocolType.VNC, protocols);
    }

    [Test]
    public void CreateProtocol_WithUnsupportedType_ThrowsException() {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => {
            _factory.CreateProtocol((ProtocolType)999);
        });
    }
}
```

## IMPLEMENTATION CHECKLIST:

### Day 1-2: Foundation Interfaces
- [ ] Create IRemoteProtocol interface
- [ ] Create ProtocolFactory implementation
- [ ] Update existing RDP implementation
- [ ] Add protocol models and enums

### Day 3-4: Database Updates
- [ ] Create EF Core migration
- [ ] Update ConnectionProfile model
- [ ] Add new model classes
- [ ] Apply database migration

### Day 5-6: Protocol Implementations
- [ ] Create SSH protocol stub
- [ ] Create VNC protocol stub
- [ ] Update dependency injection
- [ ] Test protocol factory

### Day 7-8: Testing & Validation
- [ ] Create unit tests for factory
- [ ] Create integration tests
- [ ] Validate existing functionality
- [ ] Performance testing

## SUCCESS METRICS:

✅ All foundation interfaces implemented
✅ Database schema updated successfully
✅ Protocol factory working with all protocols
✅ Existing RDP functionality preserved
✅ Unit tests passing with >80% coverage
✅ Integration tests validating multi-protocol support

## NEXT STEPS:

After completing Week 1-2 foundation, proceed to Week 3-4 core features implementation including Quick-Connect service and Performance Monitoring infrastructure.

Remember: Test each component thoroughly before moving to the next task.
