# Aivana RDP WPF - Developer Guide

**Version:** 1.0  
**Last Updated:** 2025-11-27

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Development Environment Setup](#development-environment-setup)
3. [Project Structure](#project-structure)
4. [Building the Project](#building-the-project)
5. [Running Tests](#running-tests)
6. [Development Workflow](#development-workflow)
7. [Code Organization](#code-organization)
8. [Architecture Patterns](#architecture-patterns)
9. [Adding New Features](#adding-new-features)
10. [Debugging](#debugging)
11. [Code Style Guidelines](#code-style-guidelines)

---

## Prerequisites

### Required Software

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** (Community, Professional, or Enterprise)
  - Workload: **.NET desktop development**
  - Component: **Windows 10/11 SDK**
- **Git** - For version control

### Optional Tools

- **Visual Studio Code** with C# extension (alternative IDE)
- **Windows Terminal** - Enhanced terminal experience
- **GitHub Desktop** - GUI for Git operations

### Verify Installation

```bash
# Check .NET SDK version
dotnet --version
# Should output: 8.0.x

# Check Git
git --version
```

---

## Development Environment Setup

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/Aivana-RDP-WPF.git
cd Aivana-RDP-WPF
```

### 2. Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages defined in `.csproj` files.

### 3. Build the Solution

```bash
dotnet build Aivana_RDP_WPF.sln
```

### 4. Run Database Migrations

```bash
cd Aivana_RDP_WPF
dotnet ef database update
```

This creates the SQLite database in `%AppData%\Aivana_RDP_WPF\aivana.db`.

### 5. Run the Application

```bash
dotnet run --project Aivana_RDP_WPF/Aivana_RDP_WPF.csproj
```

Or open the solution in Visual Studio and press `F5`.

---

## Project Structure

```
Aivana-RDP-WPF/
├── Aivana_RDP_WPF/                    # Main WPF application
│   ├── Models/                        # Data models (Entity Framework)
│   │   ├── ConnectionProfile.cs
│   │   ├── SessionHistory.cs
│   │   ├── ApplicationSettings.cs
│   │   └── ...
│   ├── ViewModels/                    # MVVM ViewModels
│   │   ├── MainViewModel.cs
│   │   ├── ConnectionManagement/
│   │   │   ├── ConnectionListViewModel.cs
│   │   │   ├── ConnectionConfigViewModel.cs
│   │   │   └── ConnectionSessionViewModel.cs
│   │   └── ...
│   ├── Views/                         # WPF Views (XAML)
│   │   ├── ConnectionManagement/
│   │   │   ├── ConnectionListView.xaml
│   │   │   ├── ConnectionConfigDialog.xaml
│   │   │   └── ConnectionSessionView.xaml
│   │   └── ...
│   ├── Services/                      # Business logic services
│   │   ├── IConnectionProfileService.cs
│   │   ├── ConnectionProfileService.cs
│   │   ├── IRdpConnectionService.cs
│   │   └── ...
│   ├── Infrastructure/                # Infrastructure layer
│   │   ├── Database/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── ApplicationDbContextFactory.cs
│   │   ├── Rdp/
│   │   │   └── RdpClientWrapper.cs
│   │   ├── Credentials/
│   │   │   └── WindowsCredentialManager.cs
│   │   └── Logging/
│   │       └── FileLogger.cs
│   ├── Helpers/                       # Utility classes
│   │   ├── ValidationHelper.cs
│   │   ├── FileHelper.cs
│   │   ├── NetworkHelper.cs
│   │   └── TagHelper.cs
│   ├── Converters/                    # WPF value converters
│   │   ├── CountToVisibilityConverter.cs
│   │   ├── InverseBooleanConverter.cs
│   │   └── ...
│   ├── Commands/                      # Command implementations
│   │   ├── RelayCommand.cs
│   │   └── AsyncRelayCommand.cs
│   ├── Resources/                     # XAML resources
│   │   ├── Styles/
│   │   │   └── FluentDesignStyles.xaml
│   │   └── Themes/
│   │       ├── LightTheme.xaml
│   │       └── DarkTheme.xaml
│   ├── Migrations/                    # EF Core migrations
│   ├── App.xaml                       # Application entry point
│   ├── App.xaml.cs                    # Application code-behind
│   ├── MainWindow.xaml                # Main window
│   ├── MainWindow.xaml.cs             # Main window code-behind
│   └── appsettings.json               # Configuration
├── Aivana_RDP_WPF.Tests/              # Test project
│   ├── UnitTests/
│   ├── IntegrationTests/
│   ├── SystemTests/
│   └── ...
└── docs/                              # Documentation
```

---

## Building the Project

### Command Line

```bash
# Build Debug configuration
dotnet build Aivana_RDP_WPF.sln

# Build Release configuration
dotnet build Aivana_RDP_WPF.sln -c Release

# Build without restoring packages
dotnet build --no-restore

# Clean build artifacts
dotnet clean
```

### Visual Studio

1. Open `Aivana_RDP_WPF.sln`
2. Select configuration (Debug/Release) from toolbar
3. Press `Ctrl+Shift+B` or Build → Build Solution

### Build Output

- **Debug**: `Aivana_RDP_WPF/bin/Debug/net8.0-windows/`
- **Release**: `Aivana_RDP_WPF/bin/Release/net8.0-windows/`

---

## Running Tests

### All Tests

```bash
dotnet test Aivana_RDP_WPF.Tests/Aivana_RDP_WPF.Tests.csproj
```

### Specific Test Category

```bash
# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only
dotnet test --filter Category=Integration
```

### With Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Visual Studio Test Explorer

1. Open Test Explorer (Test → Test Explorer)
2. Run All Tests or select specific tests
3. View results and code coverage

---

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Changes

- Write code following [Code Style Guidelines](#code-style-guidelines)
- Add XML documentation comments for public APIs
- Write unit tests for new functionality

### 3. Test Your Changes

```bash
# Run tests
dotnet test

# Run application
dotnet run --project Aivana_RDP_WPF/Aivana_RDP_WPF.csproj
```

### 4. Commit Changes

```bash
git add .
git commit -m "feat: Add your feature description"
```

**Commit Message Format: `type: description`

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `chore`: Build/config changes

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub.

---

## Code Organization

### MVVM Pattern

The application follows the **Model-View-ViewModel** pattern:

- **Model**: Data models (`Models/` folder)
- **View**: XAML files (`Views/` folder)
- **ViewModel**: Business logic (`ViewModels/` folder)

**Example:**

```csharp
// Model
public class ConnectionProfile { ... }

// ViewModel
public partial class ConnectionListViewModel : ObservableObject
{
    [ObservableProperty]
    private List<ConnectionProfile> _connections;
    
    [RelayCommand]
    private async Task LoadConnectionsAsync() { ... }
}

// View (XAML)
<UserControl>
    <ItemsControl ItemsSource="{Binding Connections}">
        ...
    </ItemsControl>
</UserControl>
```

### Service Layer

Business logic is separated into services:

- **Interfaces**: `IServiceName.cs` in `Services/` folder
- **Implementations**: `ServiceName.cs` in `Services/` folder
- **Registration**: In `App.xaml.cs` `ConfigureServices` method

**Example:**

```csharp
// Interface
public interface IConnectionProfileService
{
    Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default);
}

// Implementation
public class ConnectionProfileService : IConnectionProfileService
{
    private readonly ApplicationDbContext _context;
    // Implementation...
}

// Registration
services.AddScoped<IConnectionProfileService, ConnectionProfileService>();
```

### Dependency Injection

Services are registered in `App.xaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Database
    services.AddDbContext<ApplicationDbContext>(...);
    
    // Services
    services.AddScoped<IConnectionProfileService, ConnectionProfileService>();
    
    // ViewModels
    services.AddTransient<ConnectionListViewModel>();
}
```

---

## Architecture Patterns

### MVVM (Model-View-ViewModel)

- **View**: XAML files, no business logic
- **ViewModel**: Contains presentation logic, commands, properties
- **Model**: Data structures and entities

### Dependency Injection

- Uses `Microsoft.Extensions.DependencyInjection`
- Constructor injection for all dependencies
- Service lifetime: Scoped for DbContext, Transient for ViewModels

### Repository Pattern

- Entity Framework Core acts as repository
- `ApplicationDbContext` provides data access
- Services encapsulate business logic

### Command Pattern

- `RelayCommand` and `AsyncRelayCommand` for UI commands
- Commands bound to ViewModel methods
- Uses `CommunityToolkit.Mvvm.Input`

---

## Adding New Features

### 1. Create a New Service

**Step 1: Define Interface**

```csharp
// Services/IMyNewService.cs
namespace Aivana_RDP_WPF.Services;

public interface IMyNewService
{
    Task DoSomethingAsync(CancellationToken ct = default);
}
```

**Step 2: Implement Service**

```csharp
// Services/MyNewService.cs
namespace Aivana_RDP_WPF.Services;

public class MyNewService : IMyNewService
{
    private readonly ILogger<MyNewService> _logger;
    
    public MyNewService(ILogger<MyNewService> logger)
    {
        _logger = logger;
    }
    
    public Task DoSomethingAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Doing something...");
        return Task.CompletedTask;
    }
}
```

**Step 3: Register Service**

```csharp
// App.xaml.cs
services.AddScoped<IMyNewService, MyNewService>();
```

### 2. Create a New ViewModel

```csharp
// ViewModels/MyFeatureViewModel.cs
namespace Aivana_RDP_WPF.ViewModels;

public partial class MyFeatureViewModel : ObservableObject
{
    private readonly IMyNewService _service;
    private readonly ILogger<MyFeatureViewModel> _logger;
    
    [ObservableProperty]
    private string _statusMessage = "Ready";
    
    public MyFeatureViewModel(
        IMyNewService service,
        ILogger<MyFeatureViewModel> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    [RelayCommand]
    private async Task ExecuteAsync()
    {
        StatusMessage = "Executing...";
        await _service.DoSomethingAsync();
        StatusMessage = "Completed";
    }
}
```

**Register ViewModel:**

```csharp
services.AddTransient<MyFeatureViewModel>();
```

### 3. Create a New View

```xml
<!-- Views/MyFeatureView.xaml -->
<UserControl x:Class="Aivana_RDP_WPF.Views.MyFeatureView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Button Content="Execute" Command="{Binding ExecuteCommand}"/>
        <TextBlock Text="{Binding StatusMessage}"/>
    </Grid>
</UserControl>
```

```csharp
// Views/MyFeatureView.xaml.cs
public partial class MyFeatureView : UserControl
{
    public MyFeatureView()
    {
        InitializeComponent();
    }
}
```

### 4. Add Data Template

```xml
<!-- MainWindow.xaml -->
<ContentControl.Resources>
    <DataTemplate DataType="{x:Type vm:MyFeatureViewModel}">
        <views:MyFeatureView DataContext="{Binding}"/>
    </DataTemplate>
</ContentControl.Resources>
```

---

## Debugging

### Visual Studio Debugging

1. Set breakpoints (F9)
2. Start debugging (F5)
3. Step through code (F10, F11)
4. Inspect variables in Locals window

### Logging

The application uses structured logging:

```csharp
_logger.LogInformation("Connecting to {Server}:{Port}", server, port);
_logger.LogError(ex, "Error connecting to {Server}", server);
```

**Log Locations:**
- Console: Output window in Visual Studio
- File: `%AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log`

### Database Debugging

**View Database:**
- Location: `%AppData%\Aivana_RDP_WPF\aivana.db`
- Use SQLite Browser or Visual Studio SQL Server Object Explorer

**Reset Database:**
```bash
# Delete database file
rm %AppData%\Aivana_RDP_WPF\aivana.db

# Recreate
dotnet ef database update
```

---

## Code Style Guidelines

### C# Conventions

- Use **PascalCase** for classes, methods, properties
- Use **camelCase** for local variables, parameters
- Use **_camelCase** for private fields
- Use **UPPER_CASE** for constants

### Async/Await

Always use async/await for I/O operations:

```csharp
// ✅ Good
public async Task<List<ConnectionProfile>> GetAllProfilesAsync()
{
    return await _context.ConnectionProfiles.ToListAsync();
}

// ❌ Bad
public List<ConnectionProfile> GetAllProfiles()
{
    return _context.ConnectionProfiles.ToList(); // Blocks thread
}
```

### XML Documentation

Document all public APIs:

```csharp
/// <summary>
/// Retrieves all connection profiles from the database.
/// </summary>
/// <param name="ct">Cancellation token to cancel the operation.</param>
/// <returns>A list of connection profiles.</returns>
/// <exception cref="DbUpdateException">Thrown when database update fails.</exception>
public async Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default)
{
    // Implementation
}
```

### Error Handling

Use structured exception handling:

```csharp
try
{
    await _service.DoSomethingAsync();
}
catch (SpecificException ex)
{
    _logger.LogError(ex, "Specific error occurred");
    // Handle specific case
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error occurred");
    throw; // Re-throw if cannot handle
}
```

### Nullable Reference Types

The project uses nullable reference types. Always check for null:

```csharp
if (profile == null)
{
    _logger.LogWarning("Profile is null");
    return;
}

// Use null-forgiving operator only when certain
var name = profile!.Name;
```

---

## Common Tasks

### Adding a New NuGet Package

```bash
dotnet add Aivana_RDP_WPF/Aivana_RDP_WPF.csproj package PackageName
```

### Creating a Database Migration

```bash
cd Aivana_RDP_WPF
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Updating Dependencies

```bash
dotnet restore
dotnet list package --outdated
dotnet add package PackageName --version NewVersion
```

---

## Troubleshooting Development Issues

### Build Errors

**Problem:** "Package restore failed"

**Solution:**
```bash
dotnet nuget locals all --clear
dotnet restore
```

**Problem:** "Cannot find type or namespace"

**Solution:**
- Check `using` statements
- Verify project references
- Rebuild solution

### Runtime Errors

**Problem:** "Database file not found"

**Solution:**
- Ensure `%AppData%\Aivana_RDP_WPF` directory exists
- Run `dotnet ef database update`

**Problem:** "RDP ActiveX control not found"

**Solution:**
- Verify `mstscax.dll` is available (Windows system file)
- Check `AxMSTSCLib.dll` and `MSTSCLib.dll` are referenced

### Debugging Tips

1. **Enable detailed logging**: Set log level to `Debug` in `appsettings.Development.json`
2. **Check Event Viewer**: Windows Event Viewer for system-level errors
3. **Use Debugger**: Set breakpoints and step through code
4. **Check Log Files**: Review log files in `%AppData%\Aivana_RDP_WPF\Logs\`

---

## Resources

- **[Architecture Document](architecture.md)** - Detailed architecture decisions
- **[PRD](prd.md)** - Product requirements
- **[UX Design](ux-design-specification.md)** - UI/UX specifications
- **[.NET Documentation](https://docs.microsoft.com/dotnet/)** - .NET API reference
- **[WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)** - WPF guides

---

**Last Updated:** 2025-11-27

