using Skywithin.AutoRegister.DI;
using Demo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.ConsoleApp.Scenarios;

/// <summary>
/// Creates a user in one scope and reads it back in another, going through every demo layer.
/// </summary>
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
internal sealed class UserScenario(IServiceScopeFactory scopeFactory)
{
    public void Run()
    {
        Console.WriteLine("== Users: write in one scope, read in another ==");

        Guid userId;

        using (var scope = scopeFactory.CreateScope())
        {
            var created = scope.ServiceProvider.GetRequiredService<IUserService>()
                .Create("Ada Lovelace", "ada.lovelace@example.com");

            userId = created.Id;
            Console.WriteLine($"Scope 1: created {created.Name} <{created.Email}>");
        }

        using (var scope = scopeFactory.CreateScope())
        {
            var found = scope.ServiceProvider.GetRequiredService<IUserService>().GetById(userId);
            Console.WriteLine($"Scope 2: looked up by ID, found {found?.Name ?? "nothing"}");
        }

        Console.WriteLine("UserService is scoped, but the data survives because UserDataProvider is a singleton.");
    }
}
