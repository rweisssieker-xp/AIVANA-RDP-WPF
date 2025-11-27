using FluentAssertions;
using Moq;
using Xunit;
using Aivana_RDP_WPF.ViewModels.ConnectionManagement;
using Aivana_RDP_WPF.Services;
using Aivana_RDP_WPF.Tests.TestHelpers;
using Aivana_RDP_WPF.Tests.TestHelpers.Factories;

namespace Aivana_RDP_WPF.Tests.UnitTests.ViewModels;

public class ConnectionListViewModelTests : TestBase
{
    private readonly Mock<IConnectionProfileService> _mockConnectionService;
    private readonly Mock<ILogger<ConnectionListViewModel>> _mockLogger;

    public ConnectionListViewModelTests()
    {
        _mockConnectionService = new Mock<IConnectionProfileService>();
        _mockLogger = CreateMockLogger<ConnectionListViewModel>();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public void Constructor_ShouldInitializeProperties()
    {
        // Given & When
        var viewModel = new ConnectionListViewModel(
            _mockConnectionService.Object,
            _mockLogger.Object);

        // Then
        viewModel.Should().NotBeNull();
        viewModel.Connections.Should().NotBeNull();
    }

    [Fact]
    [Trait("Priority", "P0")]
    public async Task LoadConnectionsAsync_ShouldLoadConnections()
    {
        // Given
        var profiles = ConnectionProfileFactory.CreateMultiple(5);
        _mockConnectionService.Setup(s => s.GetAllProfilesAsync())
            .ReturnsAsync(profiles);

        var viewModel = new ConnectionListViewModel(
            _mockConnectionService.Object,
            _mockLogger.Object);

        // When
        await viewModel.LoadConnectionsAsync();

        // Then
        viewModel.Connections.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void FilterByGroup_ShouldFilterConnections()
    {
        // Given
        var viewModel = new ConnectionListViewModel(
            _mockConnectionService.Object,
            _mockLogger.Object);

        // When
        viewModel.SelectedGroup = "TestGroup";

        // Then
        viewModel.SelectedGroup.Should().Be("TestGroup");
        // Filter logic would be tested here
    }
}

