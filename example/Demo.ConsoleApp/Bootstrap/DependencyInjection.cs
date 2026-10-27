using Skywithin.AutoRegister.DI;
using Demo.Infrastructure.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.ConsoleApp.Bootstrap;

internal static class DependencyInjection
{
    /// <summary>
    /// Registers every demo layer, then this app's own services.
    /// </summary>
    public static IServiceCollection AddDemoConsoleApp(this IServiceCollection services)
    {
        services
            .AddDemoInfrastructure()
            .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

        return services;
    }
}
