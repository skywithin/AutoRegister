# Getting Started

This guide will help you get started with AutoRegister.DI in your .NET application.

## Installation

Install the Skywithin.AutoRegister NuGet package in your project:

```bash
dotnet add package Skywithin.AutoRegister
```

Or via Package Manager Console:

```powershell
Install-Package Skywithin.AutoRegister
```

## Quick Start

### Step 1: Decorate Your Services

Use the `[AutoRegister]` attribute to mark your services for automatic registration:

```csharp
using Skywithin.AutoRegister.DI;

// Register as interface only
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    // Implementation...
}

// Register as both interface and self
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface | RegisterAs.Self)]
public class CacheService : ICacheService
{
    // Implementation...
}

// Register as self only
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class UtilityService
{
    // Implementation...
}
```

### Step 2: Configure in Your Application

In your application startup code (typically `Program.cs` or `Startup.cs`), register services from specific assemblies:

```csharp
using Skywithin.AutoRegister.DI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAutoRegisteredServicesFromAssembly(
            typeof(Program).Assembly,
            typeof(UserService).Assembly
        );
        
        var app = builder.Build();
        app.Run();
    }
}
```

## Understanding the Attribute Parameters

### Lifetime

The `Lifetime` parameter determines how long a service instance lives:

- `Lifetime.Scoped`: One instance per scope (recommended for most services)
- `Lifetime.Transient`: New instance every time it's requested
- `Lifetime.Singleton`: Single instance for the entire application lifetime

### RegisterAs

The `RegisterAs` parameter determines how the service is registered:

- `RegisterAs.Interface`: Register as an interface. The library uses smart interface selection (filters out `IDisposable`, `IAsyncDisposable`, and `System.*` interfaces) or you can explicitly specify which interface to use.
- `RegisterAs.Self`: Register as the concrete type itself
- `RegisterAs.Interface | RegisterAs.Self`: Register as both (use bitwise OR)

## Next Steps

- Learn about [Advanced Usage](Advanced-Usage.md) for Clean Architecture patterns
- Review [Best Practices](Best-Practices.md) for optimal usage
- Explore the [API Reference](API-Reference.md) for detailed documentation
- Check out [Examples](Examples.md) for complete code samples

