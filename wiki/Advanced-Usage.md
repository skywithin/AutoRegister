# Advanced Usage

Advanced patterns and techniques for using AutoRegister.DI in complex applications.

## Clean Architecture Example

The library excels in Clean Architecture scenarios where you have multiple layers with their own services. Each layer can independently register its services using the `[AutoRegister]` attribute.

### Service Definitions

Here's how services are defined across different layers:

```csharp
// SharedKernel Layer
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class ClockService : IClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}

// Application Layer
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserService : IUserService
{
    private readonly IUserDataProvider _dataProvider;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IUserDataProvider dataProvider, ILogger<UserService> logger)
    {
        _dataProvider = dataProvider;
        _logger = logger;
    }
    // Implementation...
}

// Infrastructure Layer
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserDataProvider : IUserDataProvider
{
    // Implementation...
}

// Web API Layer
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class HealthCheckService : IHealthCheckService
{
    // Implementation...
}
```

## Layer-by-Layer Registration

Configure each layer to register its own services in a hierarchical manner. This approach ensures proper dependency ordering and maintains clean separation of concerns.

### Bootstrap Pattern

Each layer provides an extension method that registers its own services and calls the next layer's registration:

```csharp
// Web API Bootstrap
public static IServiceCollection AddApiServices(this IServiceCollection services)
{
    services
        .AddControllers()
        .AddSwagger()
        .AddDemoInfrastructure()  // Registers all lower layers
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly); // Registers Web API layer services
    
    return services;
}

// Infrastructure Bootstrap
public static IServiceCollection AddDemoInfrastructure(this IServiceCollection services)
{
    services
        .AddDemoApplication()  // Registers Application layer
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
    
    return services;
}

// Application Bootstrap
public static IServiceCollection AddDemoApplication(this IServiceCollection services)
{
    services
        .AddDemoDomain()  // Registers Domain layer
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
    
    return services;
}

// Domain Bootstrap
public static IServiceCollection AddDemoDomain(this IServiceCollection services)
{
    services
        .AddDemoKernel()  // Registers SharedKernel layer
        .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
    
    return services;
}

// SharedKernel Bootstrap
public static IServiceCollection AddDemoKernel(this IServiceCollection services)
{
    services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
    return services;
}
```

### Using AssemblyReference

To avoid hardcoding assembly references, use an `AssemblyReference` class in each project:

```csharp
// In each project
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
```

Then use it in your bootstrap methods:

```csharp
services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
```

## Dependency Chain

In a Clean Architecture setup, dependencies flow from outer layers to inner layers:

```
Web API → Infrastructure → Application → Domain → SharedKernel
```

Each layer:
1. Registers its own services using `AddAutoRegisteredServicesFromAssembly`
2. Calls the next layer's registration method
3. Maintains proper dependency ordering

## Multiple Assembly Registration

You can register services from multiple assemblies in a single call:

```csharp
builder.Services.AddAutoRegisteredServicesFromAssembly(
    null,
    typeof(Application.AssemblyReference).Assembly,
    typeof(Domain.AssemblyReference).Assembly,
    typeof(Infrastructure.AssemblyReference).Assembly
);
```

Or register them separately for better control:

```csharp
builder.Services
    .AddAutoRegisteredServicesFromAssembly(typeof(Application.AssemblyReference).Assembly)
    .AddAutoRegisteredServicesFromAssembly(typeof(Domain.AssemblyReference).Assembly)
    .AddAutoRegisteredServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly);
```

## Conditional Registration

While the `[AutoRegister]` attribute doesn't support conditional registration directly, you can achieve this by:

1. Conditionally calling `AddAutoRegisteredServicesFromAssembly` based on environment or configuration
2. Using separate assemblies for different environments
3. Filtering assemblies before registration

Example:

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAutoRegisteredServicesFromAssembly(
        null,
        typeof(DevelopmentServices.AssemblyReference).Assembly
    );
}
else
{
    builder.Services.AddAutoRegisteredServicesFromAssembly(
        null,
        typeof(ProductionServices.AssemblyReference).Assembly
    );
}
```

## Related Topics

- [Best Practices](Best-Practices.md) - Guidelines for optimal usage
- [How It Works](How-It-Works.md) - Understanding the internal mechanics
- [Examples](Examples.md) - Complete working examples

