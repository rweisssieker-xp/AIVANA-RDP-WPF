using System.IO;
using System.Windows;
using WpfApplication = System.Windows.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using Microsoft.Data.Sqlite;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.ViewModels;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Infrastructure.Logging;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Infrastructure.Protocols;

namespace Aivana_RDP_WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : WpfApplication
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build configuration
        var configuration = BuildConfiguration();

        // Configure dependency injection
        var services = new ServiceCollection();
        services.AddSingleton<IServiceProvider>(sp => sp); // Self-reference for service provider access
        ConfigureServices(services, configuration);
        _serviceProvider = services.BuildServiceProvider();

        EnsureDatabaseUpToDate(_serviceProvider);

        // Add file logger provider after service provider is built
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddProvider(new FileLoggerProvider(_serviceProvider.GetRequiredService<IOptions<FileLoggerOptions>>()));

        // Add ServiceProvider to Application Resources for access from Views
        Resources["ServiceProvider"] = _serviceProvider;

        // Create MainViewModel manually (needs IServiceProvider)
        var connectionListViewModel = _serviceProvider.GetRequiredService<ViewModels.ConnectionManagement.ConnectionListViewModel>();
        var mainViewModel = new MainViewModel(
            connectionListViewModel,
            _serviceProvider,
            _serviceProvider.GetRequiredService<ILogger<MainViewModel>>());

        // Set MainWindow DataContext with injected ViewModel
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();
    }

    private static void EnsureDatabaseUpToDate(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();

        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        EnsureColumnExists(connection, "ConnectionProfiles", "ProtocolType", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumnExists(connection, "ConnectionProfiles", "ProtocolSpecificSettings", "TEXT NULL");
        EnsureColumnExists(connection, "ConnectionProfiles", "MacAddress", "TEXT NULL");
        EnsureColumnExists(connection, "ConnectionProfiles", "EnableWakeOnLan", "BOOLEAN NOT NULL DEFAULT 0");
        EnsureColumnExists(connection, "ConnectionProfiles", "WakeTimeoutSeconds", "INTEGER NOT NULL DEFAULT 30");
        EnsureColumnExists(connection, "ConnectionProfiles", "UseSshTunnel", "BOOLEAN NOT NULL DEFAULT 0");
        EnsureColumnExists(connection, "ConnectionProfiles", "SshTunnelConfig", "TEXT NULL");
        EnsureColumnExists(connection, "ConnectionProfiles", "UpdatedAt", "TEXT NULL");

        EnsureSessionHistoriesTable(connection);
    }

    private static void EnsureSessionHistoriesTable(System.Data.Common.DbConnection connection)
    {
        using var existsCmd = connection.CreateCommand();
        existsCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SessionHistories';";
        var existing = existsCmd.ExecuteScalar() as string;
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return;
        }

        using var createCmd = connection.CreateCommand();
        createCmd.CommandText = @"
CREATE TABLE IF NOT EXISTS SessionHistories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConnectionProfileId INTEGER NOT NULL,
    ConnectedAt TEXT NOT NULL,
    DisconnectedAt TEXT NULL,
    DurationTicks INTEGER NOT NULL DEFAULT 0,
    Status TEXT NOT NULL,
    ErrorMessage TEXT NULL,
    FOREIGN KEY(ConnectionProfileId) REFERENCES ConnectionProfiles(Id) ON DELETE CASCADE
);";
        createCmd.ExecuteNonQuery();

        using var idx1 = connection.CreateCommand();
        idx1.CommandText = "CREATE INDEX IF NOT EXISTS IX_SessionHistories_ConnectionProfileId ON SessionHistories(ConnectionProfileId);";
        idx1.ExecuteNonQuery();

        using var idx2 = connection.CreateCommand();
        idx2.CommandText = "CREATE INDEX IF NOT EXISTS IX_SessionHistories_ConnectedAt ON SessionHistories(ConnectedAt);";
        idx2.ExecuteNonQuery();
    }

    private static void EnsureColumnExists(System.Data.Common.DbConnection connection, string tableName, string columnName, string columnDefinitionSql)
    {
        using var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = $"PRAGMA table_info({tableName});";

        var exists = false;
        using (var reader = pragmaCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var nameOrdinal = reader.GetOrdinal("name");
                var existingName = reader.GetString(nameOrdinal);
                if (string.Equals(existingName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
        }

        if (exists)
        {
            return;
        }

        using var alterCmd = connection.CreateCommand();
        alterCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinitionSql};";
        alterCmd.ExecuteNonQuery();
    }

    private IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add development overrides
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        if (environment == "Development")
        {
            builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        }

        // Add user settings override
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSettingsPath = Path.Combine(appDataPath, "Aivana_RDP_WPF", "appsettings.json");
        if (File.Exists(userSettingsPath))
        {
            builder.AddJsonFile(userSettingsPath, optional: true, reloadOnChange: true);
        }

        return builder.Build();
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logsDirectory = Path.Combine(appDataPath, "Aivana_RDP_WPF", "Logs");
        
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        // Configure file logger options
        services.Configure<FileLoggerOptions>(options =>
        {
            options.LogDirectory = logsDirectory;
            options.MinimumLogLevel = LogLevel.Information;
            options.RetentionDays = 30;
        });

        // Add configuration
        services.AddSingleton(configuration);

        // Configure ApplicationSettings
        services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));

        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });

        // Configure database
        var dbPath = Path.Combine(appDataPath, "Aivana_RDP_WPF", "aivana.db");
        var dbDirectory = Path.GetDirectoryName(dbPath);
        
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register services as interfaces
        services.AddScoped<IConnectionProfileService, ConnectionProfileService>();
        services.AddSingleton<IRdpConnectionService, RdpConnectionService>();
        services.AddSingleton<IFileTransferService, FileTransferService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<ISessionRecordingService, SessionRecordingService>();
        services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();
        services.AddSingleton<ICredentialService, CredentialService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IHealthCheckService, HealthCheckService>();
        services.AddScoped<ISessionHistoryService, SessionHistoryService>();

        // Multi-Protocol Support Services
        services.AddSingleton<IProtocolFactory, ProtocolFactory>();
        services.AddTransient<RdpProtocol>();
        services.AddTransient<SshProtocol>();
        services.AddTransient<VncProtocol>();

        // Week 3-4 Core Services
        services.AddSingleton<IQuickConnectService, QuickConnectService>();
        services.AddSingleton<IPerformanceMonitorService, RealPerformanceMonitorService>();
        services.AddSingleton<IWakeOnLanService, WakeOnLanService>();
        services.AddSingleton<ISshTunnelService, SshTunnelService>();

        // Phase 2 Workspace Services
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<ISessionManagerService, SessionManagerService>();
        services.AddSingleton<ILayoutPersistenceService, LayoutPersistenceService>();

        // Phase 2 Workflow Services
        services.AddSingleton<IWorkflowEngineService, WorkflowEngineService>();

        // Phase 3 AI Services
        services.AddSingleton<Aivana_RDP_WPF.Services.AI.IAIAssistantService, Aivana_RDP_WPF.Services.AI.AIAssistantService>();
        services.AddSingleton<Aivana_RDP_WPF.Services.AI.IMachineLearningService, Aivana_RDP_WPF.Services.AI.MachineLearningService>();

        // Phase 4 Analytics Services
        services.AddSingleton<Aivana_RDP_WPF.Services.Analytics.IAnalyticsService, Aivana_RDP_WPF.Services.Analytics.AnalyticsService>();
        services.AddSingleton<Aivana_RDP_WPF.Services.Analytics.IReportingService, Aivana_RDP_WPF.Services.Analytics.ReportingService>();

        // Register ViewModels
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionListViewModel>();
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionConfigViewModel>();
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionSessionViewModel>(sp =>
        {
            var rdpService = sp.GetRequiredService<IRdpConnectionService>();
            var logger = sp.GetRequiredService<ILogger<ViewModels.ConnectionManagement.ConnectionSessionViewModel>>();
            var credentialService = sp.GetService<ICredentialService>();
            var performanceMonitorService = sp.GetService<IPerformanceMonitorService>();
            var healthCheckService = sp.GetService<IHealthCheckService>();
            var sessionHistoryService = sp.GetService<ISessionHistoryService>();
            return new ViewModels.ConnectionManagement.ConnectionSessionViewModel(
                rdpService,
                logger,
                credentialService,
                performanceMonitorService,
                healthCheckService,
                sessionHistoryService);
        });
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionHealthViewModel>();
        services.AddTransient<ViewModels.SettingsViewModel>();
        services.AddTransient<ViewModels.HistoryViewModel>();
        services.AddTransient<ViewModels.HealthOverviewViewModel>();
        services.AddTransient<ViewModels.ReportsViewModel>();
        services.AddTransient<QuickConnectViewModel>();
        services.AddTransient<WorkspaceViewModel>();
        services.AddTransient<WorkflowViewModel>();
        services.AddTransient<AIAssistantViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        
        // MainViewModel needs IServiceProvider, so register after building
        services.AddSingleton<MainViewModel>(sp =>
        {
            var connectionListViewModel = sp.GetRequiredService<ViewModels.ConnectionManagement.ConnectionListViewModel>();
            return new MainViewModel(connectionListViewModel, sp, sp.GetRequiredService<ILogger<MainViewModel>>());
        });

        // Register MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

