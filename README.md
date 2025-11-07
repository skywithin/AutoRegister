# AutoRegister.DI

A powerful dependency injection automation library for .NET that simplifies service registration using attributes. This library automatically discovers and registers services decorated with the `[AutoRegister]` attribute, supporting various lifetimes and registration strategies.

## Features

- **Automatic Service Discovery**: Scans assemblies for services decorated with `[AutoRegister]` attribute
- **Flexible Lifetimes**: Supports `Scoped`, `Transient`, and `Singleton` lifetimes
- **Multiple Registration Strategies**: Register as interface, self, or both
- **Clean Architecture Support**: Perfect for layered applications with dependency hierarchies

## Quick Start

### 1. Install the Package

```bash
dotnet add package AutoRegister.DI
```

### 2. Decorate Your Services

```csharp
using Compile.AutoRegister.DI;

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

## Documentation

For comprehensive documentation, examples, API reference, and best practices, please visit the [Wiki](../wiki/Home.md).

## Target Framework

- .NET 9.0

## Contributing

This library is designed to be simple, performant, and easy to use. Contributions are welcome!
