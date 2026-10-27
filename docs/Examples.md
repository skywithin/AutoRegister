# Examples

## Registration options

```csharp
// As its interface
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class EmailService : IEmailService { }

// As a named interface: use this whenever the class has more than one candidate
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IEmailService))]
public class EmailService : IEmailService, IEmailStatus, IDisposable { }

// As itself
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class StringHelper { }

// As both (two separate registrations, so two separate instances)
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface | RegisterAs.Self)]
public class CacheService : ICacheService { }
```

## Scanning several assemblies

In a single call:

```csharp
builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(Application.AssemblyReference).Assembly,
    typeof(Infrastructure.AssemblyReference).Assembly);
```

Or chained:

```csharp
builder.Services
    .AddAutoRegisteredServicesFromAssembly(typeof(Application.AssemblyReference).Assembly)
    .AddAutoRegisteredServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly);
```

Either way, scan each assembly only once, because a second scan duplicates every registration.

## Choosing implementations per environment

The attribute has no conditions. To pick implementations per environment, put them in separate assemblies and choose which ones to scan:

```csharp
var implementations = builder.Environment.IsDevelopment()
    ? typeof(Fakes.AssemblyReference).Assembly
    : typeof(Infrastructure.AssemblyReference).Assembly;

builder.Services.AddAutoRegisteredServicesFromAssembly(implementations);
```

## Layered (Clean Architecture) setup

The repository's [`example/`](https://github.com/Skywithin/AutoRegister/tree/main/example) folder is a runnable version of this pattern:

```bash
dotnet run --project example/Demo.ConsoleApp
```

```
Demo.ConsoleApp → Demo.Infrastructure → Demo.Application → Demo.Domain → Demo.SharedKernel
```

Each project exposes its own assembly:

```csharp
// Properties/AssemblyReference.cs
public class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
```

Each project also has a bootstrap method that registers the layer below and then scans its own assembly:

```csharp
// Demo.SharedKernel/Bootstrap/DependencyInjection.cs
public static IServiceCollection AddDemoKernel(this IServiceCollection services) =>
    services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

// Demo.Application/Bootstrap/DependencyInjection.cs
public static IServiceCollection AddDemoApplication(this IServiceCollection services) =>
    services
        .AddDemoDomain()
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);

// Demo.ConsoleApp/Bootstrap/DependencyInjection.cs
public static IServiceCollection AddDemoConsoleApp(this IServiceCollection services) =>
    services
        .AddDemoInfrastructure()  // registers every lower layer
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
```

Services live in the layer that owns them. They can be `internal`, and they depend on abstractions from lower layers:

```csharp
// Demo.Domain: the abstraction
public interface IUserDataProvider { /* ... */ }

// Demo.Infrastructure: the implementation, a singleton so its in-memory data outlives each scope
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
internal sealed class UserDataProvider : IUserDataProvider { /* ... */ }

// Demo.Application: the consumer
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserService(IUserDataProvider users, IClockService clock) : IUserService { /* ... */ }
```

The host makes one call:

```csharp
var builder = Host.CreateApplicationBuilder(args);   // or WebApplication.CreateBuilder(args)
builder.Services.AddDemoConsoleApp();
```

The console app also prints instance IDs across two scopes, which shows the difference between `Transient`, `Scoped` and `Singleton`.
