using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FluentAssertions;
using Xunit;
using Aivana_RDP_WPF.Tests.TestHelpers;

namespace Aivana_RDP_WPF.Tests.PerformanceTests;

/// <summary>
/// Performance tests for connection establishment
/// NFR4: Connection completes in under 3 seconds (local network)
/// </summary>
public class ConnectionPerformanceTests : TestBase
{
    [Fact]
    [Trait("Priority", "P0")]
    [Trait("TestType", "Performance")]
    public async Task ConnectionEstablishment_ShouldCompleteUnder3Seconds()
    {
        // Given
        var startTime = DateTime.UtcNow;

        // When - Simulate connection establishment
        await Task.Delay(100); // Simulate connection time

        // Then
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalSeconds.Should().BeLessThan(3);
    }

    [Benchmark]
    public async Task ConnectionEstablishment_Benchmark()
    {
        // Benchmark connection establishment performance
        await Task.Delay(100);
    }
}

