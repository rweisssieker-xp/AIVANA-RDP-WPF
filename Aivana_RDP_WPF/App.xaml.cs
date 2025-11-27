using System.IO;
using System.Windows;
using WpfApplication = System.Windows.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.ViewModels;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Infrastructure.Logging;
using Aivana_RDP_WPF.Models;

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
        ConfigureServices(services, configuration);
        _serviceProvider = services.BuildServiceProvider();

        // Add file logger provider after service provider is built
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddProvider(new FileLoggerProvider(_serviceProvider.GetRequiredService<IOptions<FileLoggerOptions>>()));

        // Set MainWindow DataContext with injected ViewModel
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
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

        // Add file logger provider after service provider is built
        var tempProvider = services.BuildServiceProvider();
        services.AddLogging(builder =>
        {
            builder.AddProvider(new FileLoggerProvider(tempProvider.GetRequiredService<IOptions<FileLoggerOptions>>()));
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
        services.AddSingleton<INotificationService, NotificationService>();

        // Register ViewModels
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionListViewModel>();
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionConfigViewModel>();
        services.AddTransient<ViewModels.ConnectionManagement.ConnectionSessionViewModel>();
        services.AddTransient<MainViewModel>();

        // Register MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

