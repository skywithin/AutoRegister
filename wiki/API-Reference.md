# API Reference

Complete API documentation for AutoRegister.DI.

## AutoRegisterAttribute

The `AutoRegisterAttribute` is used to mark classes for automatic registration in the dependency injection container.

### Definition

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class AutoRegisterAttribute : Attribute
{
    public AutoRegisterAttribute(Lifetime serviceLifetime, RegisterAs registerAs)
    {
        ServiceLifetime = serviceLifetime;
        RegisterAs = registerAs;
        ServiceInterface = null;
    }

    public AutoRegisterAttribute(Lifetime serviceLifetime, RegisterAs registerAs, Type serviceInterface)
    {
        ServiceLifetime = serviceLifetime;
        RegisterAs = registerAs;
        ServiceInterface = serviceInterface;
    }
    
    public Lifetime ServiceLifetime { get; }
    public RegisterAs RegisterAs { get; }
    public Type? ServiceInterface { get; }
}
```

### Usage

```csharp
// Basic usage - registers as first interface
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class MyService : IMyService
{
    // Implementation
}

// Explicit interface specification
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IMyService))]
public class MyService : IDisposable, IMyService, IAsyncDisposable
{
    // Will register as IMyService, not IDisposable
}
```

### Parameters

- **serviceLifetime**: The lifetime of the service (`Lifetime.Scoped`, `Lifetime.Transient`, or `Lifetime.Singleton`)
- **registerAs**: How to register the service (`RegisterAs.Interface`, `RegisterAs.Self`, or both using bitwise OR)
- **serviceInterface** (optional): Specifies which interface to register the service as. If not provided, the first interface implemented by the class will be used. Must be an interface that the class implements.

## Lifetime Enum

Defines the service lifetime for dependency injection.

```csharp
public enum Lifetime
{
    Scoped,     // One instance per scope (e.g., per HTTP request)
    Transient,  // New instance every time
    Singleton   // Single instance for the application lifetime
}
```

### Lifetime Details

- **Scoped**: The service is created once per scope. In web applications, this typically means one instance per HTTP request. The instance is disposed when the scope ends.
- **Transient**: A new instance is created every time the service is requested. No disposal tracking is performed.
- **Singleton**: A single instance is created for the entire application lifetime. The instance is disposed when the application shuts down.

## RegisterAs Enum

Defines how a service should be registered in the dependency injection container.

```csharp
[Flags]
public enum RegisterAs
{
    Interface = 1 << 0,  // Register as an interface (uses smart selection or explicit specification)
    Self = 1 << 1        // Register as the concrete type
}
```

### RegisterAs Details

- **Interface**: Registers the service as an interface. If `ServiceInterface` is explicitly specified, that interface is used. Otherwise, the library uses smart interface selection (filters out `IDisposable`, `IAsyncDisposable`, and `System.*` interfaces) and falls back to the first suitable interface.
- **Self**: Registers the service as its concrete type.
- **Interface | Self**: Registers the service as both its interface and concrete type. Use bitwise OR to combine flags.

### Examples

```csharp
// Register only as interface (uses first interface)
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService { }

// Register only as interface with explicit specification
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : IDisposable, IUserService, IAsyncDisposable { }

// Register only as self
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class UtilityService { }

// Register as both interface and self
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface | RegisterAs.Self)]
public class CacheService : ICacheService { }
```

## Extension Methods

### AddAutoRegisteredServicesFromAssembly

Registers all services decorated with `[AutoRegister]` attribute from the specified assemblies.

```csharp
public static IServiceCollection AddAutoRegisteredServicesFromAssembly(
    this IServiceCollection services,
    params Assembly[] assemblies)
```

#### Parameters

- **services**: The `IServiceCollection` to add services to
- **assemblies**: One or more assemblies to scan for auto-registration attributes

#### Returns

Returns the `IServiceCollection` for method chaining.

#### Remarks

- At least one assembly must be specified, otherwise an `ArgumentException` is thrown
- Should be called as early as possible in the startup process before services are resolved
- This method provides better performance by limiting the scope of assembly scanning

#### Example

```csharp
builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(Program).Assembly,
    typeof(UserService).Assembly,
    typeof(DataService).Assembly
);
```

#### Behavior

1. Scans each specified assembly for classes with `[AutoRegister]` attribute
2. Filters out abstract classes and interfaces
3. For each valid service:
   - If `RegisterAs.Self` is set, registers the service as its concrete type
   - If `RegisterAs.Interface` is set:
     - Uses explicitly specified interface if provided via `ServiceInterface` parameter
     - Otherwise, uses smart interface selection (filters out `IDisposable`, `IAsyncDisposable`, and `System.*` interfaces)
     - Falls back to first interface if no suitable interface is found
     - Warns if `RegisterAs.Interface` is set but no interface is found
4. Returns the service collection for chaining
