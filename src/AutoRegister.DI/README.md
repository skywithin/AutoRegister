# AutoRegister.DI

A dependency injection automation library for .NET that simplifies service registration using attributes. This library automatically discovers and registers services decorated with the `[AutoRegister]` attribute, supporting various lifetimes and registration strategies.

## Features

- **Automatic Service Discovery**: Scans assemblies for services decorated with `[AutoRegister]` attribute
- **Flexible Lifetimes**: Supports `Scoped`, `Transient`, and `Singleton` lifetimes
- **Multiple Registration Strategies**: Register as interface, self, or both
- **Smart Interface Selection**: Automatically filters out framework interfaces like `IDisposable`
- **Explicit Interface Specification**: Optionally specify which interface to use when multiple are implemented
- **Clean Architecture Support**: Perfect for layered applications with dependency hierarchies

## Quick Start

### 1. Install the Package

```bash
dotnet add package Skywithin.AutoRegister
```

### 2. Decorate Your Services

```csharp
using Skywithin.AutoRegister.DI;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    // Implementation...
}
```

### 3. Register in Your Application

```csharp
builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(UserService).Assembly
);
```

## Advanced Usage

### Explicit Interface Specification

When a class implements multiple interfaces, you can explicitly specify which interface to use:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : IDisposable, IUserService, IAsyncDisposable
{
    // Will register as IUserService, not IDisposable
}
```

## Documentation

For comprehensive documentation, examples, API reference, and best practices, please visit the [GitHub repository](https://github.com/Skywithin/AutoRegister).

## Target Framework

- .NET 9.0

## License

MIT

## Contributing

This library is designed to be simple, performant, and easy to use. Contributions are welcome!
