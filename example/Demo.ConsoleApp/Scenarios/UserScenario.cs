using Skywithin.AutoRegister.DI;
using Demo.Application.Services;
using Demo.Domain.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.ConsoleApp.Scenarios;

/// <summary>
/// Creates a user in one scope and reads it back in another, going through every demo layer.
/// </summary>
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
internal sealed class UserScenario(IServiceScopeFactory scopeFactory)
{
    public async Task RunAsync()
    {
        Console.WriteLine("== Users: write in one scope, read in another ==");

        Guid userId;

        using (var scope = scopeFactory.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var created = await userService.CreateUserAsync(new CreateUserRequest
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada.lovelace@example.com"
            });

            userId = created.Id;
            Console.WriteLine($"Scope 1: created {created.FullName} <{created.Email}>");
        }

        using (var scope = scopeFactory.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var found = await userService.GetUserByIdAsync(userId);
            Console.WriteLine($"Scope 2: looked up by ID, found {found?.FullName ?? "nothing"}");

            var activeUsers = await userService.GetAllUsersAsync();
            Console.WriteLine($"Scope 2: active users are {string.Join(", ", activeUsers.Select(u => u.FullName))}");
        }

        Console.WriteLine("UserService is scoped, but the data survives because UserDataProvider is a singleton.");
    }
}
