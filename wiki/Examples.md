# Examples

Complete code examples demonstrating AutoRegister.DI usage patterns.

## Basic Example

A simple example showing basic service registration:

### Service Definition

```csharp
using Skywithin.AutoRegister.DI;

public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    public Task<User> GetUserAsync(int id)
    {
        // Implementation
        return Task.FromResult(new User { Id = id });
    }
}
```

### Registration

```csharp
using Skywithin.AutoRegister.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(UserService).Assembly
);

var app = builder.Build();
app.Run();
```

### Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }
}
```

## Clean Architecture Example

The repository's `example/` folder is a small Clean Architecture solution that ends in a console app. Run it with:

```bash
dotnet run --project example/Demo.ConsoleApp
```

It prints the library's registration log for every layer, then two short scenarios: one compares the three lifetimes, the other creates a user in one scope and reads it back in another.

### Project Structure

```
example/
├── Demo.SharedKernel/     # Cross-cutting services (IClockService)
├── Demo.Domain/           # Models, DTOs and abstractions (IUserDataProvider)
├── Demo.Application/      # Use cases (IUserService)
├── Demo.Infrastructure/   # Implementations of domain abstractions (UserDataProvider)
└── Demo.ConsoleApp/       # Host and entry point
```

Every layer follows the same pattern: a `Properties/AssemblyReference.cs` that exposes the layer's assembly, and a `Bootstrap/DependencyInjection.cs` extension that registers the layer below it and then scans its own assembly.

### AssemblyReference

```csharp
// Demo.SharedKernel/Properties/AssemblyReference.cs
public class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
```

### SharedKernel Layer

```csharp
// Demo.SharedKernel/Services/ClockService.cs
public interface IClockService
{
    DateTime UtcNow { get; }
}

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class ClockService : IClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}

// Demo.SharedKernel/Bootstrap/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddDemoKernel(this IServiceCollection services)
    {
        services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

        return services;
    }
}
```

### Domain Layer

The domain layer defines the abstraction that infrastructure implements. It has no `[AutoRegister]` classes of its own, but it still scans its assembly so that adding one later needs no wiring.

```csharp
// Demo.Domain/Abstractions/IUserDataProvider.cs
public interface IUserDataProvider
{
    IReadOnlyCollection<User> GetAll();
    User? Find(Guid id);
    bool TryAdd(User user);
    bool Remove(Guid id);
}

// Demo.Domain/Bootstrap/DependencyInjection.cs
public static IServiceCollection AddDemoDomain(this IServiceCollection services)
{
    services
        .AddDemoKernel()
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

    return services;
}
```

### Application Layer

`UserService` depends on abstractions from two lower layers. The container resolves both because those layers registered their implementations.

```csharp
// Demo.Application/Services/UserService.cs
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserService(
    IUserDataProvider userDataProvider,
    IClockService clock,
    ILogger<UserService> logger) : IUserService
{
    // CreateUserAsync, GetUserByIdAsync, GetAllUsersAsync, ...
}

// Demo.Application/Bootstrap/DependencyInjection.cs
public static IServiceCollection AddDemoApplication(this IServiceCollection services)
{
    services
        .AddDemoDomain()
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

    return services;
}
```

### Infrastructure Layer

The in-memory store is a singleton, so its data outlives each scope. Because one instance is shared, it uses a `ConcurrentDictionary`.

```csharp
// Demo.Infrastructure/Persistence/UserDataProvider.cs
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
internal sealed class UserDataProvider : IUserDataProvider
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public IReadOnlyCollection<User> GetAll() => _users.Values.ToList();
    public User? Find(Guid id) => _users.GetValueOrDefault(id);
    public bool TryAdd(User user) => _users.TryAdd(user.Id, user);
    public bool Remove(Guid id) => _users.TryRemove(id, out _);
}

// Demo.Infrastructure/Bootstrap/DependencyInjection.cs
public static IServiceCollection AddDemoInfrastructure(this IServiceCollection services)
{
    services
        .AddDemoApplication()
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

    return services;
}
```

### Console App Layer

The entry point registers everything with one call. The app's own services use `RegisterAs.Self`, and the lifetime demo types name their interface explicitly because each implements two.

```csharp
// Demo.ConsoleApp/Bootstrap/DependencyInjection.cs
internal static IServiceCollection AddDemoConsoleApp(this IServiceCollection services)
{
    services
        .AddDemoInfrastructure()  // Registers all lower layers
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

    return services;
}

// Demo.ConsoleApp/Lifetimes/Operations.cs
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IScopedOperation))]
internal sealed class ScopedOperation : Operation, IScopedOperation;

// Demo.ConsoleApp/Scenarios/UserScenario.cs
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
internal sealed class UserScenario(IServiceScopeFactory scopeFactory) { /* ... */ }

// Demo.ConsoleApp/Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDemoConsoleApp();

using var host = builder.Build();

host.Services.GetRequiredService<LifetimeScenario>().Run();
await host.Services.GetRequiredService<UserScenario>().RunAsync();
```

The registration code is the same in an ASP.NET Core app: call the outermost `AddDemoX()` on `builder.Services` from `WebApplication.CreateBuilder(args)`.

## Different Registration Strategies

### Register as Interface Only

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class EmailService : IEmailService
{
    // Registered as IEmailService only
}
```

### Register as Interface with Explicit Specification

When a class implements multiple interfaces, you can explicitly specify which interface to use:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IEmailService))]
public class EmailService : IDisposable, IEmailService, IAsyncDisposable
{
    // Registered as IEmailService, not IDisposable
    // This is useful when implementing framework interfaces like IDisposable
}
```

### Register as Self Only

```csharp
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class StringHelper
{
    // Registered as StringHelper only (no interface)
}
```

### Register as Both

```csharp
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface | RegisterAs.Self)]
public class CacheService : ICacheService
{
    // Registered as both ICacheService and CacheService
}
```

## Multiple Lifetimes Example

```csharp
// Scoped service (per HTTP request)
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService { }

// Transient service (new instance every time)
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class EmailValidator { }

// Singleton service (one instance for app lifetime)
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
public class ConfigurationService : IConfigurationService { }
```

## Related Topics

- [Getting Started](Getting-Started.md) - Basic setup and usage
- [Advanced Usage](Advanced-Usage.md) - Complex patterns
- [Best Practices](Best-Practices.md) - Guidelines and recommendations

