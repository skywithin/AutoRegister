using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Skywithin.AutoRegister.DI.Tests.FixtureA;
using Skywithin.AutoRegister.DI.Tests.FixtureB;
using FixtureAAssembly = Skywithin.AutoRegister.DI.Tests.FixtureA.AssemblyReference;
using FixtureBAssembly = Skywithin.AutoRegister.DI.Tests.FixtureB.AssemblyReference;

namespace Skywithin.AutoRegister.DI.Tests;

/// <summary>
/// Scans the FixtureA and FixtureB assemblies (B references A) instead of the test assembly,
/// so these fixtures never affect the registrations seen by <see cref="IoCExtensionsTests"/>.
/// </summary>
[Category("Unit")]
internal class CrossAssemblyRegistrationTests
{
    /// <summary>
    /// Verifies that scanning one assembly registers its internal and public classes,
    /// and nothing from an assembly that was not passed in.
    /// </summary>
    [Test]
    public void Scanning_FixtureA_Only_Registers_Its_Services_And_Nothing_From_FixtureB()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoRegisteredServicesFromAssembly(FixtureAAssembly.Assembly);
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert
        var fixtureAService = scope.ServiceProvider.GetService<IFixtureAService>();
        fixtureAService.Should().NotBeNull();
        fixtureAService!.GetType().Assembly.Should().BeSameAs(FixtureAAssembly.Assembly);
        fixtureAService.GetType().IsPublic.Should().BeFalse("internal classes in another assembly are registered too");

        scope.ServiceProvider.GetService<FixtureASelfService>().Should().NotBeNull();

        services.Should().NotContain(d => d.ImplementationType != null && d.ImplementationType.Assembly == FixtureBAssembly.Assembly);
        scope.ServiceProvider.GetService<ISharedContract>().Should().BeNull();
        scope.ServiceProvider.GetService<IFixtureBService>().Should().BeNull();
    }

    /// <summary>
    /// Verifies that scanning an assembly does not scan the assemblies it references, and that a class
    /// is registered under an interface declared in another assembly.
    /// </summary>
    [Test]
    public void Scanning_FixtureB_Only_Registers_Its_Services_And_Nothing_From_Referenced_FixtureA()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoRegisteredServicesFromAssembly(FixtureBAssembly.Assembly);

        // Assert
        services.Should().NotContain(d => d.ImplementationType != null && d.ImplementationType.Assembly == FixtureAAssembly.Assembly);
        services.Should().NotContain(d => d.ServiceType == typeof(IFixtureAService));
        services.Should().NotContain(d => d.ServiceType == typeof(FixtureASelfService));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IFixtureBService));

        using var serviceProvider = services.BuildServiceProvider();
        var sharedContract = serviceProvider.GetService<ISharedContract>();
        sharedContract.Should().NotBeNull();
        sharedContract!.GetType().Assembly.Should().BeSameAs(FixtureBAssembly.Assembly);
    }

    /// <summary>
    /// Verifies that scanning both assemblies in one call registers everything and resolves
    /// a dependency that crosses the assembly boundary.
    /// </summary>
    [Test]
    public void Scanning_Both_Fixtures_In_One_Call_Resolves_Cross_Assembly_Dependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoRegisteredServicesFromAssembly(FixtureAAssembly.Assembly, FixtureBAssembly.Assembly);
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert
        var fixtureBService = scope.ServiceProvider.GetService<IFixtureBService>();
        fixtureBService.Should().NotBeNull();
        fixtureBService!.Dependency.Should().BeSameAs(scope.ServiceProvider.GetRequiredService<IFixtureAService>());

        scope.ServiceProvider.GetService<FixtureASelfService>().Should().NotBeNull();
        scope.ServiceProvider.GetService<ISharedContract>().Should().NotBeNull();
    }

    /// <summary>
    /// Verifies the recommended layered pattern: an outer bootstrap method that chains the inner one
    /// registers each layer's services exactly once.
    /// </summary>
    [Test]
    public void Chained_Bootstrap_Methods_Register_Each_Service_Exactly_Once()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFixtureB();
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert
        services.Should().ContainSingle(d => d.ServiceType == typeof(IFixtureAService));
        services.Should().ContainSingle(d => d.ServiceType == typeof(FixtureASelfService));
        services.Should().ContainSingle(d => d.ServiceType == typeof(ISharedContract));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IFixtureBService));

        scope.ServiceProvider.GetService<IFixtureBService>().Should().NotBeNull();
    }
}
