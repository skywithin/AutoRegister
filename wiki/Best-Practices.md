# Best Practices

Guidelines and recommendations for using AutoRegister.DI effectively in your applications.

## 1. Use Appropriate Lifetimes

Choose the correct lifetime for each service based on its usage pattern:

### Scoped (Recommended for Most Services)

Use for services that should be shared within a request scope:

- **Repositories**: Data access services
- **Application Services**: Business logic services
- **Domain Services**: Domain-specific operations
- **Services with dependencies**: Services that depend on scoped services

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    // Shared within a single HTTP request
}
```

### Transient

Use for stateless services that can be created frequently:

- **Utility Services**: Stateless helper classes
- **Validators**: Stateless validation logic
- **Mappers**: Stateless mapping operations

```csharp
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class EmailValidator
{
    // New instance every time
}
```

### Singleton

Use for services that maintain state or are expensive to create:

- **Cache Services**: In-memory caches
- **Configuration Services**: Application configuration
- **Logging Services**: Centralized logging (though ILogger is typically scoped)
- **Expensive Initialization**: Services with heavy startup costs

```csharp
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
public class CacheService : ICacheService
{
    // Single instance for application lifetime
}
```

## 2. Interface Registration

Prefer registering as interfaces for better testability and loose coupling:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    // Implementation...
}
```

### Benefits

- **Testability**: Easy to mock interfaces in unit tests
- **Loose Coupling**: Depend on abstractions, not concrete types
- **Flexibility**: Swap implementations without changing dependent code
- **Dependency Inversion**: Follows SOLID principles

### Explicit Interface Specification

When a class implements multiple interfaces, explicitly specify which interface to use:

```csharp
// Problem: May register as IDisposable instead of IUserService
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IDisposable, IUserService, IAsyncDisposable
{
    // Implementation...
}

// Solution: Explicitly specify the intended interface
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : IDisposable, IUserService, IAsyncDisposable
{
    // Implementation...
}
```

**When to use explicit interface specification:**

- Classes implementing framework interfaces (`IDisposable`, `IAsyncDisposable`, etc.)
- Classes implementing multiple business interfaces where the first one isn't the intended one
- When you want to be explicit about which interface is the primary service contract

**When not to use explicit interface specification:**

- Classes implementing a single interface (unnecessary)
- When the first interface is the intended one (backward compatible default behavior)

### When to Use Self Registration

Use `RegisterAs.Self` for:

- Internal utility classes that don't need interfaces
- Services that are only used internally within a layer
- Simple helper classes

```csharp
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
internal class StringHelper
{
    // Internal utility, no interface needed
}
```

## 3. Assembly Organization

Organize your services by layer and register them appropriately:

### Recommended Structure

```csharp
// Register each layer's services in order
services
    .AddSharedKernelServices()      // SharedKernel services
    .AddDomainServices()            // Domain services  
    .AddApplicationServices()       // Application services
    .AddInfrastructureServices()    // Infrastructure services
    .AddWebServices();              // Web API services
```

### Benefits

- **Clear Dependencies**: Outer layers depend on inner layers
- **Maintainability**: Easy to understand service registration flow
- **Testability**: Each layer can be tested independently
- **Performance**: Only scan necessary assemblies

## 4. Performance Considerations

### Assembly-Specific Scanning

Always specify which assemblies to scan rather than scanning all loaded assemblies:

```csharp
// Good: Specific assemblies
builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(MyService).Assembly
);

// Avoid: Scanning all assemblies (if such a method existed)
// This would be slower and less predictable
```

### Minimize Assembly Scanning

Only scan assemblies that contain services you need:

```csharp
// Scan only necessary assemblies
builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(Application.AssemblyReference).Assembly,
    typeof(Domain.AssemblyReference).Assembly
    // Don't scan third-party assemblies
);
```

### Registration Order

Call `AddAutoRegisteredServicesFromAssembly` as early as possible in your startup process, before services are resolved:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register framework services first
    services.AddControllers();
    services.AddSwagger();
    
    // Register auto-registered services early (before other third-party services that might depend on them)
    services.AddAutoRegisteredServicesFromAssembly(
        typeof(AssemblyReference).Assembly
    );
    
    // Register other third-party services
}
```

## 5. Naming Conventions

Follow consistent naming patterns:

- **Services**: Use descriptive names ending with "Service" (e.g., `UserService`, `EmailService`)
- **Interfaces**: Prefix with "I" (e.g., `IUserService`, `IEmailService`)
- **Repositories**: Use descriptive names ending with "Repository" or "DataProvider"
- **Internal Services**: Mark as `internal` when they shouldn't be used outside the assembly

## 6. Error Prevention

### Validate Service Configuration

The library automatically validates:
- Services are not abstract
- Services are not interfaces
- At least one assembly is provided

### Handle Missing Interfaces

If you use `RegisterAs.Interface` but the class has no interface, the library will warn you:

```
WARNING! [MyService] has no interface to register as. Skipping interface registration.
```

To fix this, either:
- Add an interface to the class
- Use `RegisterAs.Self` instead
- Use `RegisterAs.Interface | RegisterAs.Self` to register as self when no interface exists

## 7. Testing Considerations

### Mocking Services

When services are registered as interfaces, they're easy to mock:

```csharp
// In your tests
var mockUserService = new Mock<IUserService>();
services.AddScoped<IUserService>(_ => mockUserService.Object);
```

### Test-Specific Registration

For integration tests, you might want to register different implementations:

```csharp
// In test setup
if (IsTestEnvironment)
{
    services.AddScoped<IUserService, TestUserService>();
}
else
{
    services.AddAutoRegisteredServicesFromAssembly(
        typeof(AssemblyReference).Assembly
    );
}
```

## 8. Documentation

Document complex service registrations:

```csharp
/// <summary>
/// Registers all application layer services.
/// Services are automatically discovered using [AutoRegister] attributes.
/// </summary>
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services)
{
    return services.AddAutoRegisteredServicesFromAssembly(
        AssemblyReference.Assembly
    );
}
```

## Related Topics

- [Getting Started](Getting-Started.md) - Basic usage patterns
- [Advanced Usage](Advanced-Usage.md) - Complex scenarios
- [API Reference](API-Reference.md) - Complete API documentation

