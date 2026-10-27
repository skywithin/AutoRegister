using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Skywithin.AutoRegister.DI.Tests.FixtureA;

public interface IFixtureAService { }

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class FixtureAService : IFixtureAService { }

[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public sealed class FixtureASelfService { }

// Implemented in FixtureB, to test a class and its interface living in different assemblies.
public interface ISharedContract { }

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}

public static class DependencyInjection
{
    public static IServiceCollection AddFixtureA(this IServiceCollection services) =>
        services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
}
