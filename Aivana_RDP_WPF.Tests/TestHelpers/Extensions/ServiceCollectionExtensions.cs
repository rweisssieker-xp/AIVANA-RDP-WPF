using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Extensions;

/// <summary>
/// Extension methods for test service collection setup
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds test services to the service collection
    /// </summary>
    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise in test output
        });

        // Add in-memory database
        services.AddDbContext<Aivana_RDP_WPF.Infrastructure.Database.ApplicationDbContext>(
            options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        return services;
    }

    /// <summary>
    /// Adds mocked services for unit testing
    /// </summary>
    public static IServiceCollection AddMockedServices(this IServiceCollection services)
    {
        // Add mocked services here as they are implemented
        // Example:
        // services.AddSingleton(Mock.Of<IConnectionProfileService>());
        
        return services;
    }
}

