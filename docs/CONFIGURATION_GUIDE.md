# Aivana RDP WPF - Configuration Guide

**Version:** 1.0  
**Last Updated:** 2025-11-27

---

## Table of Contents

1. [Configuration Files](#configuration-files)
2. [Application Settings](#application-settings)
3. [Connection Settings](#connection-settings)
4. [Performance Configuration](#performance-configuration)
5. [Logging Configuration](#logging-configuration)
6. [Database Configuration](#database-configuration)
7. [Environment Variables](#environment-variables)
8. [User-Specific Settings](#user-specific-settings)

---

## Configuration Files

### File Locations

- **Default Configuration**: `Aivana_RDP_WPF/appsettings.json`
- **Development Overrides**: `Aivana_RDP_WPF/appsettings.Development.json`
- **User-Specific**: `%AppData%\Aivana_RDP_WPF\appsettings.User.json` (created automatically)

### Configuration Hierarchy

Settings are loaded in this order (later files override earlier):

1. `appsettings.json` (base configuration)
2. `appsettings.Development.json` (development overrides, if exists)
3. `appsettings.User.json` (user-specific overrides, if exists)

---

## Application Settings

### Default Configuration (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "File": {
      "LogDirectory": "%AppData%\\Aivana_RDP_WPF\\Logs",
      "FileName": "aivana-{Date}.log",
      "RetentionDays": 30
    }
  },
  "Database": {
    "ConnectionString": "Data Source=%AppData%\\Aivana_RDP_WPF\\aivana.db"
  },
  "Application": {
    "DefaultConnection": {
      "Port": 3389,
      "ColorDepth": 32,
      "Resolution": "1920x1080"
    },
    "Performance": {
      "MaxConcurrentConnections": 10,
      "ConnectionTimeout": 30000
    },
    "UI": {
      "Theme": "Light",
      "Language": "en-US"
    }
  }
}
```

---

## Connection Settings

### Default Connection Settings

Configure default values for new connections:

```json
{
  "Application": {
    "DefaultConnection": {
      "Port": 3389,           // Default RDP port
      "ColorDepth": 32,       // 16, 24, or 32 bits
      "Resolution": "1920x1080"  // Common resolutions: 1920x1080, 1680x1050, 1280x720, "Full Screen"
    }
  }
}
```

### Connection-Specific Settings

Each connection profile can override default settings. Settings are stored in the `Settings` JSON field of `ConnectionProfile`:

```json
{
  "Resolution": "1920x1080",
  "ColorDepth": 32,
  "Compression": true,
  "BitmapCaching": true,
  "AutoReconnect": true
}
```

---

## Performance Configuration

### Max Concurrent Connections

Limit the number of simultaneous RDP connections:

```json
{
  "Application": {
    "Performance": {
      "MaxConcurrentConnections": 10  // Maximum number of simultaneous connections
    }
  }
}
```

**Recommendations:**
- **Low-end machines**: 3-5 connections
- **Mid-range machines**: 5-10 connections
- **High-end machines**: 10-20 connections

### Connection Timeout

Set timeout for connection attempts:

```json
{
  "Application": {
    "Performance": {
      "ConnectionTimeout": 30000  // Milliseconds (30 seconds)
    }
  }
}
```

---

## Logging Configuration

### Log Levels

Configure logging verbosity:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",           // Trace, Debug, Information, Warning, Error, Critical
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Aivana_RDP_WPF": "Debug"          // Application-specific logging
    }
  }
}
```

**Log Levels:**
- **Trace**: Very detailed logs (development only)
- **Debug**: Detailed diagnostic information
- **Information**: General application flow
- **Warning**: Warning messages
- **Error**: Error messages
- **Critical**: Critical failures

### File Logging

Configure file logging location and retention:

```json
{
  "Logging": {
    "File": {
      "LogDirectory": "%AppData%\\Aivana_RDP_WPF\\Logs",
      "FileName": "aivana-{Date}.log",
      "RetentionDays": 30  // Delete logs older than 30 days
    }
  }
}
```

**Special Variables:**
- `%AppData%` - Expands to `C:\Users\{Username}\AppData\Roaming`
- `{Date}` - Replaced with current date (YYYY-MM-DD)

---

## Database Configuration

### Connection String

SQLite database location:

```json
{
  "Database": {
    "ConnectionString": "Data Source=%AppData%\\Aivana_RDP_WPF\\aivana.db"
  }
}
```

**Default Location:** `C:\Users\{Username}\AppData\Roaming\Aivana_RDP_WPF\aivana.db`

### Database Options

Entity Framework Core options (configured in code):

- **Migrations Assembly**: `Aivana_RDP_WPF`
- **SQLite Compatibility**: SQLite 3.x
- **Connection Pooling**: Enabled by default

---

## Environment Variables

### Override Configuration

Set environment variables to override configuration:

**Windows PowerShell:**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Logging__LogLevel__Default = "Debug"
```

**Windows Command Prompt:**
```cmd
set ASPNETCORE_ENVIRONMENT=Development
set Logging__LogLevel__Default=Debug
```

**Environment Variable Format:**
- Use double underscore `__` for nested properties
- Example: `Logging__LogLevel__Default` maps to `Logging.LogLevel.Default`

### Common Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development`, `Production` |
| `Logging__LogLevel__Default` | Default log level | `Debug`, `Information` |
| `Database__ConnectionString` | Database connection | `Data Source=C:\Custom\path.db` |

---

## User-Specific Settings

### User Settings File

User-specific settings are stored in:
`%AppData%\Aivana_RDP_WPF\appsettings.User.json`

This file is created automatically when you change settings in the UI.

**Example:**
```json
{
  "Application": {
    "UI": {
      "Theme": "Dark"
    },
    "Performance": {
      "MaxConcurrentConnections": 5
    }
  }
}
```

### Editing User Settings

**Method 1: Via Application UI**
1. Open Settings menu
2. Change settings
3. Settings are saved automatically

**Method 2: Manual Edit**
1. Navigate to `%AppData%\Aivana_RDP_WPF\`
2. Open `appsettings.User.json` in a text editor
3. Modify settings
4. Restart application

---

## Advanced Configuration

### Custom Logging Providers

Add custom logging providers in `App.xaml.cs`:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddFile(options =>
    {
        options.LogDirectory = "%AppData%\\Aivana_RDP_WPF\\Logs";
        options.FileName = "aivana-{Date}.log";
        options.RetentionDays = 30;
    });
});
```

### Database Migrations

Configure migration behavior:

```bash
# Create migration
dotnet ef migrations add MigrationName --project Aivana_RDP_WPF

# Apply migration
dotnet ef database update --project Aivana_RDP_WPF

# Revert migration
dotnet ef database update PreviousMigrationName --project Aivana_RDP_WPF
```

---

## Configuration Best Practices

### Development

- Use `appsettings.Development.json` for development-specific settings
- Set log level to `Debug` for detailed diagnostics
- Enable verbose logging for troubleshooting

### Production

- Use default `appsettings.json` for production
- Set log level to `Information` or `Warning`
- Configure appropriate connection limits
- Set up log retention policies

### Security

- **Never commit** `appsettings.User.json` to version control
- **Never store** passwords in configuration files (use Windows Credential Manager)
- **Use environment variables** for sensitive configuration in production

---

## Troubleshooting Configuration

### Configuration Not Loading

**Problem:** Settings changes not taking effect

**Solutions:**
1. Restart the application
2. Check file syntax (valid JSON)
3. Verify file location
4. Check environment variable overrides

### Database Connection Issues

**Problem:** Cannot connect to database

**Solutions:**
1. Verify `%AppData%\Aivana_RDP_WPF` directory exists
2. Check file permissions
3. Verify connection string syntax
4. Check disk space

### Logging Not Working

**Problem:** No log files created

**Solutions:**
1. Verify log directory exists
2. Check write permissions
3. Verify log level configuration
4. Check `appsettings.json` syntax

---

## Configuration Reference

### Complete Configuration Schema

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "string",
      "Microsoft": "string",
      "Microsoft.Hosting.Lifetime": "string"
    },
    "File": {
      "LogDirectory": "string",
      "FileName": "string",
      "RetentionDays": "number"
    }
  },
  "Database": {
    "ConnectionString": "string"
  },
  "Application": {
    "DefaultConnection": {
      "Port": "number",
      "ColorDepth": "number",
      "Resolution": "string"
    },
    "Performance": {
      "MaxConcurrentConnections": "number",
      "ConnectionTimeout": "number"
    },
    "UI": {
      "Theme": "string",
      "Language": "string"
    }
  }
}
```

---

## Examples

### Development Configuration

`appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Aivana_RDP_WPF": "Trace"
    }
  },
  "Application": {
    "Performance": {
      "MaxConcurrentConnections": 3
    }
  }
}
```

### Production Configuration

`appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    },
    "File": {
      "RetentionDays": 7
    }
  },
  "Application": {
    "Performance": {
      "MaxConcurrentConnections": 10,
      "ConnectionTimeout": 60000
    }
  }
}
```

---

**Last Updated:** 2025-11-27

