# AutoRegister.DI

[![NuGet](https://img.shields.io/nuget/v/Skywithin.AutoRegister.svg)](https://www.nuget.org/packages/Skywithin.AutoRegister/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Register .NET services by putting an attribute on the class. `AddAutoRegisteredServicesFromAssembly` scans the assemblies you give it and adds every class marked `[AutoRegister]` to the `IServiceCollection`.

## Quick start

```bash
dotnet add package Skywithin.AutoRegister
```

```csharp
using Skywithin.AutoRegister.DI;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService { }
```

```csharp
builder.Services.AddAutoRegisteredServicesFromAssembly(typeof(UserService).Assembly);
```

## Features

- **Lifetimes:** `Scoped`, `Transient` and `Singleton`.
- **Register as the interface, the class itself, or both:** `RegisterAs.Interface`, `RegisterAs.Self`, or `RegisterAs.Interface | RegisterAs.Self`.
- **Interface selection:** without an explicit interface, the library skips `IDisposable`, `IAsyncDisposable` and `System.*` interfaces when choosing one. Name the interface when a class has several:

  ```csharp
  [AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
  public class UserService : IUserService, IAuditable, IDisposable { }
  ```

- **Explicit assembly list:** only the assemblies you pass are scanned, which suits layered (Clean Architecture) solutions where each project registers its own services.
- **Visible results:** every registration, warning and error is printed to the console. An invalid attribute is logged and skipped instead of throwing.

## Documentation

- [Getting Started](https://github.com/Skywithin/AutoRegister/blob/main/docs/Getting-Started.md)
- [API Reference](https://github.com/Skywithin/AutoRegister/blob/main/docs/API-Reference.md): the attribute, the enums, and the exact registration rules
- [Examples](https://github.com/Skywithin/AutoRegister/blob/main/docs/Examples.md): registration options and a layered setup
- [Best Practices](https://github.com/Skywithin/AutoRegister/blob/main/docs/Best-Practices.md): includes the architectural trade-offs
- [Troubleshooting](https://github.com/Skywithin/AutoRegister/blob/main/docs/Troubleshooting.md): every console message explained
- [Version Management](https://github.com/Skywithin/AutoRegister/blob/main/docs/Version-Management.md): how releases are published

A runnable layered example lives in [`example/`](https://github.com/Skywithin/AutoRegister/tree/main/example): `dotnet run --project example/Demo.ConsoleApp`.

## Trade-offs

The attribute only declares how a class should be registered; `AddAutoRegisteredServicesFromAssembly` does the registering. This removes most composition-root boilerplate. The costs are that every decorated project references this package, and registrations are spread across classes instead of living in one file. If your domain layer must stay free of infrastructure references, keep the attribute out of it, or register those services manually. The two approaches mix freely.

## Requirements

.NET 10. The only dependency is `Microsoft.Extensions.DependencyInjection.Abstractions`.

## License

MIT
