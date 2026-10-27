# Getting Started

## Install

```bash
dotnet add package Skywithin.AutoRegister
```

The package targets .NET 10 and depends only on `Microsoft.Extensions.DependencyInjection.Abstractions`.

## Mark your classes

```csharp
using Skywithin.AutoRegister.DI;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService { }

[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class EmailValidator { }
```

- `Lifetime` is `Scoped`, `Transient` or `Singleton`, matching the standard `ServiceLifetime` values.
- `RegisterAs.Interface` registers the class as one of its interfaces, and `RegisterAs.Self` registers it as its own type. You can combine them: `RegisterAs.Interface | RegisterAs.Self`.

## Scan the assemblies

```csharp
using Skywithin.AutoRegister.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoRegisteredServicesFromAssembly(typeof(UserService).Assembly);
```

Pass every assembly that contains `[AutoRegister]` classes. The call works the same with any `IServiceCollection`: ASP.NET Core, the generic host, or a plain `new ServiceCollection()`.

## Resolve as usual

```csharp
public class UsersController(IUserService users) : ControllerBase { }
```

When the scan runs, the library prints each registration to the console, so you can see what was registered. See [API Reference](API-Reference.md#console-output).

## Next

- [API Reference](API-Reference.md): the attribute, the enums, and the exact registration rules
- [Examples](Examples.md): registration options and a layered (Clean Architecture) setup
- [Best Practices](Best-Practices.md)
- [Troubleshooting](Troubleshooting.md)
