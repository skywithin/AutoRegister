# Features

AutoRegister.DI provides a powerful and flexible dependency injection automation solution for .NET applications.

## Core Features

### Automatic Service Discovery

The library automatically scans specified assemblies for services decorated with the `[AutoRegister]` attribute. This eliminates the need for manual service registration in your dependency injection container configuration.

### Flexible Lifetimes

Supports all standard .NET dependency injection lifetimes:

- **Scoped**: One instance per scope (e.g., per HTTP request)
- **Transient**: New instance every time
- **Singleton**: Single instance for the application lifetime

### Multiple Registration Strategies

Register services in three different ways:

- **Interface Only**: Register the service as its interface (most common)
- **Self Only**: Register the service as its concrete type
- **Both**: Register the service as both interface and concrete type

### Smart Interface Selection

Automatically filters out framework interfaces that are typically not meant for DI registration:
- Excludes `IDisposable` and `IAsyncDisposable`
- Excludes interfaces from `System.*` namespaces
- Falls back to first interface if no suitable interface is found
- Can explicitly specify which interface to use when multiple are implemented

### Explicit Interface Specification

When a class implements multiple interfaces, you can explicitly specify which interface to register:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : IDisposable, IUserService, IAsyncDisposable
{
    // Will register as IUserService, not IDisposable
}
```

### Assembly-Specific Scanning

Target specific assemblies for better performance. Instead of scanning all loaded assemblies, you can specify exactly which assemblies to scan, reducing startup time and improving performance.

### Clean Architecture Support

Perfect for layered applications with dependency hierarchies. The library works seamlessly with Clean Architecture patterns, allowing each layer to register its own services independently.

## Benefits

- **Reduced Boilerplate**: No need to manually register each service
- **Type Safety**: Compile-time checking of service registrations
- **Performance**: Assembly-specific scanning for optimal performance
- **Flexibility**: Support for various registration strategies and lifetimes
- **Maintainability**: Clear, declarative service registration using attributes

