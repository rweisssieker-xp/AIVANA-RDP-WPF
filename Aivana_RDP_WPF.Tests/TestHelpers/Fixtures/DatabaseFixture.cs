using Microsoft.EntityFrameworkCore;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Tests.TestHelpers.Factories;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Fixtures;

/// <summary>
/// Fixture for in-memory database testing
/// </summary>
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(_options);
        Context.Database.EnsureCreated();
    }

    public void SeedTestData()
    {
        // Seed connection profiles
        var profiles = new List<ConnectionProfile>
        {
            ConnectionProfileFactory.Create("Test Server 1", "192.168.1.100"),
            ConnectionProfileFactory.Create("Test Server 2", "192.168.1.101"),
            ConnectionProfileFactory.Create("Favorite Server", "192.168.1.102", isFavorite: true)
        };

        Context.ConnectionProfiles.AddRange(profiles);
        Context.SaveChanges();
    }

    public void ClearDatabase()
    {
        Context.ConnectionProfiles.RemoveRange(Context.ConnectionProfiles);
        Context.SessionHistory.RemoveRange(Context.SessionHistory);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}

