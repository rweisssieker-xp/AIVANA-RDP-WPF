using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Tests.TestHelpers;
using Aivana_RDP_WPF.Tests.TestHelpers.Factories;
using Aivana_RDP_WPF.Tests.TestHelpers.Fixtures;

namespace Aivana_RDP_WPF.Tests.IntegrationTests.Database;

public class DatabaseContextTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ApplicationDbContext _context;

    public DatabaseContextTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.Context;
        _fixture.ClearDatabase();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void DatabaseContext_CreatesSuccessfully()
    {
        // Given & When
        var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        // Then
        context.Should().NotBeNull();
        context.Database.EnsureCreated().Should().BeTrue();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task ConnectionProfile_CanBeAdded()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Test Server", "192.168.1.100");

        // When
        _context.ConnectionProfiles.Add(profile);
        var result = await _context.SaveChangesAsync();

        // Then
        result.Should().BeGreaterThan(0);
        _context.ConnectionProfiles.Should().Contain(p => p.Id == profile.Id);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task SessionHistory_CanBeAdded()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Test Server", "192.168.1.100");
        _context.ConnectionProfiles.Add(profile);
        await _context.SaveChangesAsync();

        var history = SessionHistoryFactory.Create(profile.Id);

        // When
        _context.SessionHistory.Add(history);
        var result = await _context.SaveChangesAsync();

        // Then
        result.Should().BeGreaterThan(0);
        _context.SessionHistory.Should().Contain(h => h.Id == history.Id);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ConnectionProfile_WithTags_StoresAsJson()
    {
        // Given
        var tags = new List<string> { "production", "critical", "backup" };
        var profile = ConnectionProfileFactory.Create(
            "Test Server",
            "192.168.1.100",
            tags: tags);

        // When
        _context.ConnectionProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Then
        var saved = await _context.ConnectionProfiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id);
        
        saved.Should().NotBeNull();
        saved.Tags.Should().HaveCount(3);
        saved.Tags.Should().Contain("production");
    }
}

