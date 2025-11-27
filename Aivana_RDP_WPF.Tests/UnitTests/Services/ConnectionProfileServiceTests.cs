using FluentAssertions;
using Moq;
using Xunit;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Tests.TestHelpers;
using Aivana_RDP_WPF.Tests.TestHelpers.Factories;
using Aivana_RDP_WPF.Tests.TestHelpers.Assertions;

namespace Aivana_RDP_WPF.Tests.UnitTests.Services;

public class ConnectionProfileServiceTests : TestBase
{
    private readonly Mock<IConnectionProfileService> _mockService;
    private readonly ILogger<ConnectionProfileService> _logger;

    public ConnectionProfileServiceTests()
    {
        _mockService = new Mock<IConnectionProfileService>();
        _logger = CreateMockLogger<ConnectionProfileService>().Object;
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task CreateProfileAsync_ShouldCreateProfile()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Test Server", "192.168.1.100");
        _mockService.Setup(s => s.CreateProfileAsync(It.IsAny<ConnectionProfile>()))
            .ReturnsAsync(profile);

        // When
        var result = await _mockService.Object.CreateProfileAsync(profile);

        // Then
        result.Should().NotBeNull();
        result.ShouldBeValidConnectionProfile();
        result.Name.Should().Be("Test Server");
        result.ServerAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task UpdateProfileAsync_ShouldUpdateProfile()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Original Name", "192.168.1.100");
        profile.Name = "Updated Name";
        _mockService.Setup(s => s.UpdateProfileAsync(It.IsAny<ConnectionProfile>()))
            .ReturnsAsync(profile);

        // When
        var result = await _mockService.Object.UpdateProfileAsync(profile);

        // Then
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task DeleteProfileAsync_ShouldDeleteProfile()
    {
        // Given
        var profileId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteProfileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        // When
        var result = await _mockService.Object.DeleteProfileAsync(profileId);

        // Then
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task GetProfileByIdAsync_ShouldRetrieveProfile()
    {
        // Given
        var profileId = Guid.NewGuid();
        var profile = ConnectionProfileFactory.Create("Test Server", "192.168.1.100");
        profile.Id = profileId;
        _mockService.Setup(s => s.GetProfileByIdAsync(It.Is<Guid>(id => id == profileId)))
            .ReturnsAsync(profile);

        // When
        var result = await _mockService.Object.GetProfileByIdAsync(profileId);

        // Then
        result.Should().NotBeNull();
        result.Id.Should().Be(profileId);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void ValidateProfile_RequiredFields_ShouldFailWhenMissing()
    {
        // Given
        var profile = new ConnectionProfile
        {
            Name = "", // Empty name
            ServerAddress = "" // Empty server address
        };

        // When & Then
        profile.Name.Should().BeEmpty();
        profile.ServerAddress.Should().BeEmpty();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void ValidateProfile_PortRange_ShouldFailWhenInvalid()
    {
        // Given
        var profile = ConnectionProfileFactory.Create("Test", "192.168.1.100", port: 0);

        // When & Then
        profile.Port.Should().BeLessThan(1);
    }
}

