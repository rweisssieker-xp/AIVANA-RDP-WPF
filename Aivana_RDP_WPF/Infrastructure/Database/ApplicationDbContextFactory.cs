using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Aivana_RDP_WPF.Infrastructure.Database;

/// <summary>
/// Design-time factory for Entity Framework Core migrations.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a default path for design-time migrations
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbDirectory = Path.Combine(appDataPath, "Aivana_RDP_WPF");
        var dbPath = Path.Combine(dbDirectory, "aivana.db");
        
        // Ensure directory exists
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

