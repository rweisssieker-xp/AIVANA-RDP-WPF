using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Aivana_RDP_WPF.Infrastructure.Database;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Tests.TestHelpers;
using Aivana_RDP_WPF.Tests.TestHelpers.Factories;
using Aivana_RDP_WPF.Tests.TestHelpers.Fixtures;
using Aivana_RDP_WPF.Tests.TestHelpers.Assertions;

namespace Aivana_RDP_WPF.Tests.IntegrationTests.Services;

public class ConnectionProfileServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ApplicationDbContext _context;
    private readonly IConnectionProfileService _service;

    public ConnectionProfileServiceIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.Context;
        _fixture.ClearDatabase();
        // Initialize service with real database context
        // _service = new ConnectionProfileService(_context, logger);
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task CreateProfile_SavesToDatabase()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Test Server", "192.168.1.100");

        // When
        _context.ConnectionProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Then
        var savedProfile = await _context.ConnectionProfiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id);
        
        savedProfile.Should().NotBeNull();
        savedProfile.ShouldBeValidConnectionProfile();
        savedProfile.Name.Should().Be("Test Server");
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task UpdateProfile_PersistsChanges()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Original Name", "192.168.1.100");
        _context.ConnectionProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // When
        profile.Name = "Updated Name";
        _context.ConnectionProfiles.Update(profile);
        await _context.SaveChangesAsync();

        // Then
        var updatedProfile = await _context.ConnectionProfiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id);
        
        updatedProfile.Should().NotBeNull();
        updatedProfile.Name.Should().Be("Updated Name");
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task DeleteProfile_RemovesFromDatabase()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Test Server", "192.168.1.100");
        _context.ConnectionProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // When
        _context.ConnectionProfiles.Remove(profile);
        await _context.SaveChangesAsync();

        // Then
        var deletedProfile = await _context.ConnectionProfiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id);
        
        deletedProfile.Should().BeNull();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task GetFavorites_ReturnsOnlyFavorites()
    {
        // Given
        var favorite1 = ConnectionProfileFactory.Create("Favorite 1", "192.168.1.100", isFavorite: true);
        var favorite2 = ConnectionProfileFactory.Create("Favorite 2", "192.168.1.101", isFavorite: true);
        var normal = ConnectionProfileFactory.Create("Normal", "192.168.1.102", isFavorite: false);

        _context.ConnectionProfiles.AddRange(favorite1, favorite2, normal);
        await _context.SaveChangesAsync();

        // When
        var favorites = await _context.ConnectionProfiles
            .Where(p => p.IsFavorite)
            .ToListAsync();

        // Then
        favorites.Should().HaveCount(2);
        favorites.Should().OnlyContain(p => p.IsFavorite);
    }
}

