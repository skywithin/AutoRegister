using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Skywithin.AutoRegister.DI.Tests.FixtureA;

namespace Skywithin.AutoRegister.DI.Tests.FixtureB;

[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
internal sealed class SharedContractImplementation : ISharedContract { }

public interface IFixtureBService
{
    IFixtureAService Dependency { get; }
}

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class FixtureBService(IFixtureAService dependency) : IFixtureBService
{
    public IFixtureAService Dependency { get; } = dependency;
}

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}

public static class DependencyInjection
{
    // Chains the lower layer first, like the example's bootstrap methods.
    public static IServiceCollection AddFixtureB(this IServiceCollection services) =>
        services
            .AddFixtureA()
            .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
}
