# How It Works

Understanding the internal mechanics of AutoRegister.DI.

## Overview

AutoRegister.DI uses reflection to discover and register services at application startup. The process involves scanning assemblies, identifying decorated classes, and registering them with the dependency injection container.

## Service Discovery

The library scans specified assemblies for classes decorated with the `[AutoRegister]` attribute:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    // This service will be automatically registered
}
```

### Discovery Process

1. **Assembly Scanning**: The library iterates through all types in the specified assemblies
2. **Attribute Detection**: Filters types that have the `[AutoRegister]` attribute
3. **Validation**: Excludes abstract classes and interfaces
4. **Type Extraction**: Extracts the service lifetime, registration strategy, and interface information

### Code Flow

The library uses a `ServiceRegistration` record to hold registration information:

```csharp
private record ServiceRegistration(
    Lifetime ServiceLifetime,
    RegisterAs RegisterAs,
    Type? ServiceInterface,
    Type Implementation);
```

The discovery process:

```csharp
var registrations = assemblies
    .SelectMany(assembly => assembly.GetTypes())
    .Where(type =>
        type.IsDefined(typeof(AutoRegisterAttribute), inherit: false) &&
        !type.IsAbstract &&
        !type.IsInterface)
    .Select(type => TryCreateRegistration(type))
    .Where(registration => registration != null)
    .Cast<ServiceRegistration>();
```

## Registration Process

For each discovered service, the library performs the following steps:

### 1. Validation

The library validates that:
- The type is not abstract
- The type is not an interface
- At least one assembly was provided

### 2. Registration Strategy

Based on the `RegisterAs` parameter, the library determines how to register the service:

#### RegisterAs.Self

Registers the service as its concrete type:

```csharp
if (type.RegisterAs.HasFlag(RegisterAs.Self))
{
    services.AddScoped(type.Implementation, type.Implementation);
}
```

#### RegisterAs.Interface

Registers the service as an interface. The library uses smart interface selection:

1. If `ServiceInterface` is explicitly specified in the attribute, use that interface
2. Otherwise, use `GetSuitableInterface()` which:
   - Filters out `IDisposable` and `IAsyncDisposable`
   - Filters out interfaces from `System.*` namespaces
   - Returns the first suitable interface
   - Falls back to first interface if no suitable interface is found

```csharp
if (registration.RegisterAs.HasFlag(RegisterAs.Interface))
{
    if (registration.ServiceInterface == null)
    {
        // Warn and skip interface registration
        return;
    }
    services.AddScoped(registration.ServiceInterface, registration.Implementation);
}
```

#### RegisterAs.Interface | RegisterAs.Self

Registers the service as both its interface and concrete type (both conditions above are executed).

### 3. Lifetime Registration

The library registers the service with the appropriate lifetime:

```csharp
switch (lifetime)
{
    case Lifetime.Scoped:
        services.AddScoped(serviceType, implementationType);
        break;
    case Lifetime.Transient:
        services.AddTransient(serviceType, implementationType);
        break;
    case Lifetime.Singleton:
        services.AddSingleton(serviceType, implementationType);
        break;
}
```

## Dependency Chain

In a Clean Architecture setup, dependencies flow from outer layers to inner layers:

```
Web API → Infrastructure → Application → Domain → SharedKernel
```

### Registration Flow

1. **Web API Layer** calls `AddApiServices()`
   - Registers Web API services
   - Calls `AddDemoInfrastructure()`

2. **Infrastructure Layer** calls `AddDemoInfrastructure()`
   - Registers Infrastructure services
   - Calls `AddDemoApplication()`

3. **Application Layer** calls `AddDemoApplication()`
   - Registers Application services
   - Calls `AddDemoDomain()`

4. **Domain Layer** calls `AddDemoDomain()`
   - Registers Domain services
   - Calls `AddDemoKernel()`

5. **SharedKernel Layer** calls `AddDemoKernel()`
   - Registers SharedKernel services
   - No further dependencies

This ensures that inner layers are registered before outer layers, maintaining proper dependency ordering.

## Service Resolution

When a service is requested from the dependency injection container, the following happens:

### 1. Dependency Resolution

The DI container:
- Looks up the registered service type
- Creates an instance of the implementation type
- Recursively resolves all constructor dependencies
- Injects dependencies into the constructor

### 2. Lifetime Management

The container manages service lifetimes:

- **Scoped**: Creates one instance per scope, reuses it within the scope, disposes when scope ends
- **Transient**: Creates a new instance every time, no disposal tracking
- **Singleton**: Creates one instance, reuses it for the application lifetime, disposes on application shutdown

### 3. Disposal

Services implementing `IDisposable` or `IAsyncDisposable` are properly disposed:

- **Scoped**: Disposed when the scope ends
- **Singleton**: Disposed when the application shuts down
- **Transient**: Not tracked for disposal (disposed immediately if not referenced)

## Performance Considerations

### Assembly Scanning

The library only scans the assemblies you specify, which provides:

- **Better Performance**: Avoids scanning unnecessary assemblies
- **Predictable Behavior**: Only registers services from known assemblies
- **Faster Startup**: Reduces reflection overhead

### Reflection Overhead

The reflection-based discovery happens only once at application startup:

- **Startup Cost**: One-time cost during application initialization
- **Runtime Performance**: No runtime reflection overhead
- **Caching**: The DI container caches service registrations

### Optimization Tips

1. **Limit Assembly Scanning**: Only scan assemblies that contain services
2. **Register Early**: Call `AddAutoRegisteredServicesFromAssembly` as early as possible in startup
3. **Use Specific Assemblies**: Avoid scanning large third-party assemblies

## Error Handling

The library includes comprehensive error handling:

### Validation Errors

- **No Assemblies**: Throws `ArgumentException` if no assemblies are provided
- **Abstract Classes**: Automatically filtered out (no error)
- **Interfaces**: Automatically filtered out (no error)

### Warnings

- **Missing Interface**: Warns when `RegisterAs.Interface` is used but no interface exists
  ```
  WARNING! [MyService] has no interface to register as. Skipping interface registration.
  ```

### Console Output

The library provides colored console output:
- **Yellow**: Assembly scanning messages
- **Green**: Successful service registrations
- **Red**: Warnings and errors
- **Cyan**: Summary information

## Related Topics

- [API Reference](API-Reference.md) - Detailed API documentation
- [Best Practices](Best-Practices.md) - Performance and usage guidelines
- [Troubleshooting](Troubleshooting.md) - Common issues and solutions

