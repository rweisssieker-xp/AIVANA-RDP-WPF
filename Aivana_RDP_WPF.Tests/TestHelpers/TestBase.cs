using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Aivana_RDP_WPF.Tests.TestHelpers.Fixtures;

namespace Aivana_RDP_WPF.Tests.TestHelpers;

/// <summary>
/// Base class for test classes with common setup
/// </summary>
public abstract class TestBase : IDisposable
{
    protected IServiceProvider ServiceProvider { get; private set; }
    protected DatabaseFixture? DatabaseFixture { get; private set; }
    
    protected Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();

    protected TestBase()
    {
        DatabaseFixture = new DatabaseFixture();
        ServiceProvider = BuildServiceProvider();
    }

    protected virtual IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add database context
        services.AddDbContext<Aivana_RDP_WPF.Infrastructure.Database.ApplicationDbContext>(
            options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        
        // Add other services as needed
        // This will be extended as services are implemented
        
        return services.BuildServiceProvider();
    }

    public void Dispose()
    {
        DatabaseFixture?.Dispose();
        (ServiceProvider as IDisposable)?.Dispose();
    }
}

