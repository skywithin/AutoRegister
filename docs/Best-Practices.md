# Best Practices

## Pick lifetimes deliberately

- **Scoped** is the right default for application services, repositories and anything that depends on a scoped service.
- **Transient** suits lightweight, stateless helpers.
- **Singleton** suits caches, in-memory stores and expensive-to-build services. A singleton is shared across threads, so it must be thread-safe, and it must not depend on scoped services.

## Prefer interfaces, and name them when it's ambiguous

Registering as an interface keeps consumers decoupled and easy to fake in tests. If the class implements more than one interface (counting those inherited from base classes, and `System` ones like `IEquatable<T>`), pass the interface explicitly. Automatic selection then can't pick the wrong one:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : AuditedService, IUserService { }
```

Use `RegisterAs.Self` for helpers that have no abstraction.

## Avoid `Interface | Self` on stateful services

The two registrations are independent, so a `Singleton` or `Scoped` class registered both ways yields two instances. If you need one shared instance under both types, register the class as `Self` and forward the interface manually. See [Troubleshooting](Troubleshooting.md#class-and-interface-resolve-to-different-instances).

## Scan only your own assemblies, each exactly once

- Use an `AssemblyReference` class per project rather than `Assembly.GetExecutingAssembly()` or a type picked at random.
- Don't scan third-party assemblies. That is wasted work and risks a `ReflectionTypeLoadException`.
- A second scan of the same assembly duplicates every registration. In a layered setup, let each layer's bootstrap method scan only its own assembly. See [Examples](Examples.md#layered-clean-architecture-setup).

## Overriding registrations in tests

Registrations are plain `ServiceDescriptor`s, so the usual rules apply. Register a replacement **after** the scan, and it wins for single resolves:

```csharp
services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
services.AddScoped<IEmailSender, FakeEmailSender>();
```

## Mixing with manual registration

Use the attribute for routine services. Register by hand anything that needs a factory, options, open generics or a forwarded instance:

```csharp
services.AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
services.AddScoped<ISpecialService>(sp =>
    new SpecialService(sp.GetRequiredService<IOptions<SpecialOptions>>().Value));
```

## Know the trade-off

The attribute is metadata: the class declares how it wants to be registered, and the scanning code does the registering. That removes most composition-root boilerplate, but every decorated project now references this package, and the registrations are spread across classes instead of living in one place.

That trade-off suits most small and medium applications. If your domain layer must stay free of infrastructure references, or you want every registration visible in one file, keep the attribute out of that layer, or register manually.
