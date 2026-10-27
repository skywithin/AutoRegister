# API Reference

Namespace: `Skywithin.AutoRegister.DI`

## AutoRegisterAttribute

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class AutoRegisterAttribute : Attribute
{
    public AutoRegisterAttribute(Lifetime serviceLifetime, RegisterAs registerAs);
    public AutoRegisterAttribute(Lifetime serviceLifetime, RegisterAs registerAs, Type serviceInterface);

    public Lifetime ServiceLifetime { get; }
    public RegisterAs RegisterAs { get; }
    public Type? ServiceInterface { get; }
}
```

| Parameter | Meaning |
|-----------|---------|
| `serviceLifetime` | The lifetime of every registration made for the class. |
| `registerAs` | Which registrations to make; see [RegisterAs](#registeras). |
| `serviceInterface` | Optional. The interface to register when `RegisterAs.Interface` is set. It must be an interface that the class implements. It is ignored when `RegisterAs.Interface` is not set. |

## Lifetime

```csharp
public enum Lifetime { Scoped, Transient, Singleton }
```

These map to `AddScoped`, `AddTransient` and `AddSingleton`, and the container's normal lifetime and disposal rules apply.

## RegisterAs

```csharp
[Flags]
public enum RegisterAs { Interface = 1, Self = 2 }
```

| Value | Registrations |
|-------|---------------|
| `Interface` | One registration, with the interface as the service type. See [Interface selection](#interface-selection). |
| `Self` | One registration, with the class as the service type. |
| `Interface \| Self` | Two independent registrations. They are not forwarded to a shared instance, so for `Scoped` and `Singleton`, resolving the class and resolving the interface give you **different instances**. |

## AddAutoRegisteredServicesFromAssembly

```csharp
public static IServiceCollection AddAutoRegisteredServicesFromAssembly(
    this IServiceCollection services,
    params Assembly[] assemblies)
```

The method scans the given assemblies, registers every eligible class, and returns `services` so you can chain calls. Throws `ArgumentException` if `assemblies` is `null` or empty.

### Which classes are registered

A type is registered when all of these hold:

- `[AutoRegister]` is declared directly on it. The attribute is not inherited: a subclass of a decorated class is not registered.
- It is a concrete class, so not abstract (which also excludes static classes) and not an interface.
- It passes the validation below.

Visibility doesn't matter: `internal` and nested classes are registered too.

### Interface selection

When `RegisterAs.Interface` is set:

1. **If `serviceInterface` is given**, it is used. If it is not an interface, or the class doesn't implement it, the library logs an error and skips the class entirely, including any `Self` registration.
2. **Otherwise** the library looks at all interfaces of the class, including ones inherited from base classes and base interfaces, and removes:
   - `IDisposable` and `IAsyncDisposable`
   - interfaces whose namespace starts with `System.` (for example `System.Collections.Generic`)

   It then picks the first one that remains. Interfaces declared directly in the `System` namespace, such as `IEquatable<T>`, `IComparable<T>` and `ICloneable`, are **not** removed.
3. If nothing remains after filtering, it falls back to the first interface of any kind, which can be `IDisposable`.
4. If the class has no interfaces at all, it logs a warning and skips the interface registration. A `Self` registration is still made if requested.

The order of `Type.GetInterfaces()` is not guaranteed. **If a class has more than one candidate interface, pass `serviceInterface`.**

### Other rules

- `RegisterAs` of `0` logs a warning, and the class is skipped.
- Registrations use `Add*`, not `TryAdd*`. Scanning the same assembly twice registers everything twice. When several registrations share a service type, the last one wins for a single resolve, and `IEnumerable<T>` returns all of them.
- Generic classes such as `Repository<T>` are not supported. The registration they produce makes `BuildServiceProvider` throw. Register open generics manually: `services.AddScoped(typeof(IRepository<>), typeof(Repository<>))`.

### Exceptions

| Exception | When |
|-----------|------|
| `ArgumentException` | `assemblies` is `null` or empty. |
| `NullReferenceException` | An element of `assemblies` is `null`. |
| `ReflectionTypeLoadException` | A type in a scanned assembly can't be loaded. |

Every other invalid configuration is logged and skipped. Nothing else throws.

### Console output

The library always writes to standard output with `Console.WriteLine`, in colour. This can't be turned off.

| Colour | Message |
|--------|---------|
| Cyan | `Scanning <Assembly> (<Version>) assembly for auto-registrations...` (one line per assembly) |
| Green | `Registering [<Class>] as [<Interface>\|self] with [<Lifetime>] lifetime.` |
| Yellow | Warnings, see [Troubleshooting](Troubleshooting.md) |
| Red | Errors, see [Troubleshooting](Troubleshooting.md) |
| Yellow | `Auto-registration completed. Registered <N> service(s).` |

Type names are full names, including the namespace. `N` counts the classes that passed validation, not the registrations made: a class with `Interface | Self` counts once.
