# Troubleshooting

Common issues, error messages, and solutions when using AutoRegister.DI.

## Common Issues

### Services Not Being Registered

**Symptoms**: Services decorated with `[AutoRegister]` are not found or cannot be resolved.

**Possible Causes**:

1. **Assembly Not Scanned**: The assembly containing the service is not included in the `AddAutoRegisteredServicesFromAssembly` call.

   **Solution**: Ensure the assembly is included:
   ```csharp
   builder.Services.AddAutoRegisteredServicesFromAssembly(
       typeof(MyService).Assembly  // Make sure this assembly is included
   );
   ```

2. **Service is Abstract or Interface**: Abstract classes and interfaces are automatically excluded.

   **Solution**: Ensure the service is a concrete class:
   ```csharp
   // Wrong - abstract class
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public abstract class BaseService : IService { }

   // Correct - concrete class
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public class MyService : IService { }
   ```

3. **Attribute Not Applied**: The `[AutoRegister]` attribute is missing or incorrectly applied.

   **Solution**: Verify the attribute is present and correctly formatted:
   ```csharp
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public class MyService : IMyService { }
   ```

### Interface Registration Warning

**Warning Message**:
```
WARNING! [MyService] has no interface to register as. Skipping interface registration.
```

**Cause**: The service is decorated with `RegisterAs.Interface` but doesn't implement any interface.

**Solutions**:

1. **Add an Interface** (Recommended):
   ```csharp
   public interface IMyService { }
   
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public class MyService : IMyService { }
   ```

2. **Use RegisterAs.Self**:
   ```csharp
   [AutoRegister(Lifetime.Transient, RegisterAs.Self)]
   public class MyService { }
   ```

3. **Use Both**:
   ```csharp
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface | RegisterAs.Self)]
   public class MyService { }
   ```

### Wrong Interface Registered

**Symptoms**: Service is registered as an unexpected interface (e.g., `IDisposable` instead of `IUserService`).

**Cause**: When a class implements multiple interfaces, the library uses the first interface by default, which may not be the intended one.

**Solution**: Explicitly specify which interface to use:

```csharp
// Problem: Registers as IDisposable (first interface)
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IDisposable, IUserService { }

// Solution: Explicitly specify IUserService
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUserService))]
public class UserService : IDisposable, IUserService { }
```

### Invalid Interface Specification Error

**Error Message**:
```
ERROR! [MyService] does not implement the specified interface [IUnimplementedInterface]. Skipping registration.
```

**Cause**: The `ServiceInterface` parameter specifies an interface that the class doesn't implement.

**Solution**: Ensure the specified interface is actually implemented by the class:

```csharp
// Wrong - IUnimplementedInterface is not implemented
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IUnimplementedInterface))]
public class MyService : IMyService { }

// Correct - IMyService is implemented
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IMyService))]
public class MyService : IMyService { }
```

### Non-Interface Type Error

**Error Message**:
```
ERROR! [MyService] specifies [ServiceA] as ServiceInterface, but it is not an interface. Skipping registration.
```

**Cause**: The `ServiceInterface` parameter specifies a type that is not an interface.

**Solution**: Only specify interface types:

```csharp
// Wrong - ServiceA is a class, not an interface
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(ServiceA))]
public class MyService : IMyService { }

// Correct - IMyService is an interface
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IMyService))]
public class MyService : IMyService { }
```

### ArgumentException: At least one assembly must be specified

**Error Message**:
```
System.ArgumentException: At least one assembly must be specified.
```

**Cause**: `AddAutoRegisteredServicesFromAssembly` was called without any assembly parameters.

**Solution**: Provide at least one assembly:
```csharp
// Wrong
builder.Services.AddAutoRegisteredServicesFromAssembly();

// Correct
builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(MyService).Assembly
);
```

### Service Resolution Fails

**Symptoms**: `InvalidOperationException` when trying to resolve a service, even though it's registered.

**Possible Causes**:

1. **Wrong Service Type**: Trying to resolve a concrete type when it's only registered as an interface (or vice versa).

   **Solution**: Resolve using the registered type:
   ```csharp
   // If registered as RegisterAs.Interface
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public class MyService : IMyService { }

   // Correct - resolve as interface
   var service = serviceProvider.GetRequiredService<IMyService>();

   // Wrong - won't work if only registered as interface
   var service = serviceProvider.GetRequiredService<MyService>();
   ```

2. **Missing Dependencies**: The service has constructor dependencies that aren't registered.

   **Solution**: Ensure all dependencies are registered:
   ```csharp
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public class MyService : IMyService
   {
       public MyService(IDependency dependency) { }
   }

   // Make sure IDependency is also registered
   [AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
   public class Dependency : IDependency { }
   ```

3. **Circular Dependencies**: Services depend on each other in a cycle.

   **Solution**: Refactor to break the circular dependency, possibly using an intermediary service or event-based communication.

## Performance Issues

### Slow Application Startup

**Symptoms**: Application takes a long time to start.

**Possible Causes**:

1. **Scanning Large Assemblies**: Scanning assemblies with many types.

   **Solution**: Only scan necessary assemblies:
   ```csharp
   // Avoid scanning large third-party assemblies
   builder.Services.AddAutoRegisteredServicesFromAssembly(
       typeof(MyService).Assembly  // Only your assemblies
   );
   ```

2. **Too Many Services**: Registering a very large number of services.

   **Solution**: This is usually not a problem, but if startup is slow, consider:
   - Lazy loading for some services
   - Conditional registration
   - Reviewing if all services are necessary

### Memory Usage

The library has minimal memory overhead:
- Service registrations are stored in the DI container (standard .NET behavior)
- Reflection happens only at startup
- No runtime reflection overhead

## Best Practices for Troubleshooting

### Verify Registration

Check what services are registered:

```csharp
var app = builder.Build();

// In development, you can inspect registered services
if (app.Environment.IsDevelopment())
{
    var serviceProvider = app.Services;
    // Use a service provider inspector or logging to see registered services
}
```

### Test in Isolation

Test service registration in a simple console app:

```csharp
var services = new ServiceCollection();

services.AddAutoRegisteredServicesFromAssembly(
    typeof(MyService).Assembly
);

var serviceProvider = services.BuildServiceProvider();

// Try to resolve the service
var service = serviceProvider.GetRequiredService<IMyService>();
```

## Getting Help

If you encounter issues not covered here:

1. Check the [API Reference](API-Reference.md) for correct usage
2. Review [Best Practices](Best-Practices.md) for common patterns
3. Examine the [Examples](Examples.md) for working code samples
4. Check the console output for specific error messages

## Related Topics

- [Getting Started](Getting-Started.md) - Basic setup guide
- [API Reference](API-Reference.md) - Complete API documentation
- [Best Practices](Best-Practices.md) - Usage guidelines

