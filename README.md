# AutoRegister.DI

[![NuGet](https://img.shields.io/nuget/v/Skywithin.AutoRegister.svg)](https://www.nuget.org/packages/Skywithin.AutoRegister/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

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

## Architectural Considerations and Trade-offs

### SOLID Principles and Single Responsibility

Some developers have concerns that using the `[AutoRegister]` attribute violates SOLID principles, particularly the Single Responsibility Principle (SRP). However, it's important to understand the design philosophy:

**The attribute is a marker, not the registration logic itself.**

- The `[AutoRegister]` attribute serves as a **declarative marker** that indicates a class should be registered
- The **actual registration logic** is separate and handled by the `AddAutoRegisteredServicesFromAssembly` method
- The class itself doesn't contain registration logic—it only declares its registration intent
- This separation maintains SRP: the class has one responsibility (its business logic), and the registration system has another (service registration)

```csharp
// The attribute is just metadata - it doesn't execute registration logic
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    // This class has one responsibility: user business logic
    // The registration happens elsewhere, separately
}
```

### Domain and Infrastructure Coupling

**Acknowledged concern:** Using the `[AutoRegister]` attribute does create a dependency between your domain/application code and the dependency injection infrastructure.

**The trade-off:**
- **Convenience**: Automatic registration reduces boilerplate and maintenance overhead
- **Coupling**: Your domain/application code becomes aware of the DI infrastructure
- **Impact**: This is generally acceptable for small to medium-sized projects where the convenience benefits outweigh the architectural purity concerns

**When this matters:**
- In **large enterprise applications** with strict architectural boundaries, this coupling might be a concern
- If your organization has strict policies about domain layer independence, you may want to consider manual registration
- For projects where architectural purity is paramount, explicit registration in composition roots may be preferred

### Convenience vs. Strict Architecture

AutoRegister.DI is designed with a **pragmatic approach** that prioritizes developer productivity and maintainability:

**Works well for:**
- Small to medium-sized projects
- Projects where rapid development is important
- Teams that value reduced boilerplate
- Applications where the convenience benefits outweigh strict architectural boundaries

**Consider alternatives for:**
- Large enterprise applications with strict architectural requirements
- Projects where domain layer must remain completely infrastructure-agnostic
- Situations where explicit control over registration is required
- Teams that prefer explicit, visible registration code

### Making an Informed Decision

**Use AutoRegister.DI when:**
- You want to reduce boilerplate and maintenance overhead
- Your project size is small to medium
- The convenience benefits outweigh architectural purity concerns
- Your team is comfortable with the attribute-based approach

**Consider manual registration when:**
- You have strict architectural boundaries that must be maintained
- Your domain layer must remain completely infrastructure-agnostic
- You need explicit, visible control over all registrations
- Your project is large enough that the coupling concerns outweigh convenience benefits

**Hybrid approach:**
You can also use a hybrid approach—use `[AutoRegister]` for most services, but manually register services that have special requirements or need explicit control:

```csharp
// Use AutoRegister for standard services
services.AddAutoRegisteredServicesFromAssembly(
    typeof(Application.AssemblyReference).Assembly
);

// Manually register services with special requirements
services.AddScoped<ISpecialService>(sp => 
    new SpecialService(sp.GetRequiredService<IOptions<SpecialConfig>>()));
```

### Summary

AutoRegister.DI makes a **pragmatic trade-off** between architectural purity and developer convenience. It's designed to work well for most projects, but acknowledges that for very large projects with strict architectural requirements, the coupling concerns might outweigh the benefits. The choice is yours based on your project's specific needs and priorities.

## Target Framework

- .NET 9.0

## License

MIT

## Contributing

This library is designed to be simple, performant, and easy to use. Contributions are welcome!
