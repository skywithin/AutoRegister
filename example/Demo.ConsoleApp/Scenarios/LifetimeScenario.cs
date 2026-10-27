using Skywithin.AutoRegister.DI;
using Demo.ConsoleApp.Lifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.ConsoleApp.Scenarios;

/// <summary>
/// Resolves each lifetime twice in each of two scopes and prints the instance IDs.
/// </summary>
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
internal sealed class LifetimeScenario(IServiceScopeFactory scopeFactory)
{
    public void Run()
    {
        Console.WriteLine("== Lifetimes: two resolves in each of two scopes ==");

        for (var scopeNumber = 1; scopeNumber <= 2; scopeNumber++)
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            for (var resolve = 1; resolve <= 2; resolve++)
            {
                Console.WriteLine(
                    $"Scope {scopeNumber}, resolve {resolve}:  " +
                    $"transient {ShortId<ITransientOperation>(provider)}  " +
                    $"scoped {ShortId<IScopedOperation>(provider)}  " +
                    $"singleton {ShortId<ISingletonOperation>(provider)}");
            }
        }

        Console.WriteLine("Transient changes on every resolve, scoped changes per scope, singleton never changes.");
        Console.WriteLine();
    }

    private static string ShortId<T>(IServiceProvider provider) where T : IOperation =>
        provider.GetRequiredService<T>().Id.ToString()[..8];
}
