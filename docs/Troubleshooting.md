# Troubleshooting

Start with the console output: every scan lists what it registered and why anything was skipped. In the messages below, `<Class>` and `<Interface>` are full type names.

## Console messages

### `WARNING! [<Class>] has RegisterAs set to None (0). Skipping registration.`

`registerAs` was `0`, for example `(RegisterAs)0`. Use `RegisterAs.Interface`, `RegisterAs.Self` or both.

### `WARNING! [<Class>] has RegisterAs.Interface flag but implements no suitable interfaces. Skipping interface registration.`

The class implements no interfaces, so there is nothing to register it as. This line is followed by a second warning ending in `...but no interface to register. Skipping interface registration.` To fix it, add an interface or use `RegisterAs.Self`. If the class also has `Self` set, that registration is still made.

### `ERROR! [<Class>] specifies [<Type>] as ServiceInterface, but it is not an interface. Skipping registration.`

The third attribute argument is a class or struct. Pass an interface.

### `ERROR! [<Class>] does not implement the specified interface [<Interface>]. Skipping registration.`

The class doesn't implement the interface you named. Fix the attribute, or implement the interface.

Both `ERROR!` cases skip the whole class, including a `Self` registration.

## Symptoms

### A service can't be resolved

- **The assembly wasn't scanned.** Check the `Scanning ...` lines and add the missing assembly.
- **The attribute is on a base class.** The attribute is not inherited, so put it on the concrete class.
- **The class is abstract.** Abstract classes are skipped without a message.
- **You're resolving the wrong type.** `RegisterAs.Interface` registers only the interface, and `RegisterAs.Self` registers only the class.
- **A constructor dependency isn't registered.** The container's exception names the missing type.

### The wrong interface was registered

When a class has more than one candidate interface, the choice depends on `Type.GetInterfaces()` order, which isn't guaranteed and includes interfaces from base classes. Interfaces in the `System` namespace itself, such as `IEquatable<T>`, are also candidates. Name the interface explicitly:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : BaseService, IEquatable<UserService>, IUserService { }
```

### Every service is registered twice

The same assembly was scanned more than once, for example in two layers' bootstrap methods or twice in one call. Scan each assembly exactly once.

### `ArgumentException: Cannot instantiate implementation type 'Repository`1[T]'`

`[AutoRegister]` is on a generic class, and these are not supported. Remove the attribute and register the open generic manually:

```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

### Class and interface resolve to different instances

`RegisterAs.Interface | RegisterAs.Self` makes two separate registrations. To share one instance, register the class with `RegisterAs.Self` and forward the interface manually:

```csharp
services.AddSingleton<ICache>(sp => sp.GetRequiredService<Cache>());
```

### `ArgumentException: At least one assembly must be specified.`

`AddAutoRegisteredServicesFromAssembly()` was called with no assemblies, or with `null`.

### `ReflectionTypeLoadException`

A scanned assembly contains a type whose dependencies can't be loaded. Scan only your own assemblies, and check that their dependencies are deployed.
