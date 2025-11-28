using FluentAssertions;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Assertions;

/// <summary>
/// Custom assertions for domain models
/// </summary>
public static class CustomAssertions
{
    public static void ShouldBeValidConnectionProfile(this ConnectionProfile profile)
    {
        profile.Should().NotBeNull();
        profile.Name.Should().NotBeNullOrWhiteSpace();
        profile.ServerAddress.Should().NotBeNullOrWhiteSpace();
        profile.Port.Should().BeInRange(1, 65535);
        profile.Id.Should().BeGreaterThanOrEqualTo(0);
    }

    public static void ShouldHaveValidSessionHistory(this SessionHistory history)
    {
        history.Should().NotBeNull();
        history.ConnectionProfileId.Should().BeGreaterThan(0);
        history.ConnectedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        history.Status.Should().NotBeNullOrWhiteSpace();
    }

    public static void ShouldHaveValidPerformanceMetrics(this PerformanceMetrics metrics)
    {
        metrics.Should().NotBeNull();
        metrics.ConnectionProfileId.Should().BeGreaterThanOrEqualTo(0);
        metrics.LatencyMs.Should().BeGreaterThanOrEqualTo(0);
        metrics.BandwidthMbps.Should().BeGreaterThanOrEqualTo(0);
        metrics.PacketLossPercent.Should().BeInRange(0, 100);
        metrics.FrameRate.Should().BeGreaterThanOrEqualTo(0);
    }
}
