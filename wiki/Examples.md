# Examples

Complete code examples demonstrating AutoRegister.DI usage patterns.

## Basic Example

A simple example showing basic service registration:

### Service Definition

```csharp
using Skywithin.AutoRegister.DI;

public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService
{
    public Task<User> GetUserAsync(int id)
    {
        // Implementation
        return Task.FromResult(new User { Id = id });
    }
}
```

### Registration

```csharp
using Skywithin.AutoRegister.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoRegisteredServicesFromAssembly(
    typeof(UserService).Assembly
);

var app = builder.Build();
app.Run();
```

### Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }
}
```

## Clean Architecture Example

A complete Clean Architecture example with multiple layers:

### Project Structure

```
example/
├── Demo.SharedKernel/     # Shared domain concepts
├── Demo.Domain/           # Business logic and models
├── Demo.Application/      # Use cases and application services
├── Demo.Infrastructure/   # External concerns (data, APIs)
└── Demo.Web.Api/          # Presentation layer
```

### SharedKernel Layer

```csharp
// Demo.SharedKernel/Services/IClockService.cs
public interface IClockService
{
    DateTime UtcNow { get; }
}

// Demo.SharedKernel/Services/ClockService.cs
using Skywithin.AutoRegister.DI;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class ClockService : IClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}

// Demo.SharedKernel/Bootstrap/DependencyInjection.cs
using Skywithin.AutoRegister.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddDemoKernel(
        this IServiceCollection services)
    {
        services.AddAutoRegisteredServicesFromAssembly(
            AssemblyReference.Assembly);
        
        return services;
    }
}

// Demo.SharedKernel/Properties/AssemblyReference.cs
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
```

### Domain Layer

```csharp
// Demo.Domain/Abstractions/IUserDataProvider.cs
public interface IUserDataProvider
{
    Task<User> GetUserAsync(int id);
    Task<User> CreateUserAsync(CreateUserRequest request);
}

// Demo.Domain/Models/User.cs
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Demo.Domain/Bootstrap/DependencyInjection.cs
using Skywithin.AutoRegister.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddDemoDomain(
        this IServiceCollection services)
    {
        services
            .AddDemoKernel()  // Register SharedKernel first
            .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
        
        return services;
    }
}
```

### Application Layer

```csharp
// Demo.Application/Services/IUserService.cs
public interface IUserService
{
    Task<UserResponse> GetUserAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
}

// Demo.Application/Services/UserService.cs
using Skywithin.AutoRegister.DI;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserService : IUserService
{
    private readonly IUserDataProvider _dataProvider;
    private readonly IClockService _clockService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserDataProvider dataProvider,
        IClockService clockService,
        ILogger<UserService> logger)
    {
        _dataProvider = dataProvider;
        _clockService = clockService;
        _logger = logger;
    }

    public async Task<UserResponse> GetUserAsync(int id)
    {
        _logger.LogInformation("Getting user {UserId} at {Time}", 
            id, _clockService.UtcNow);
        
        var user = await _dataProvider.GetUserAsync(id);
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = await _dataProvider.CreateUserAsync(request);
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}

// Demo.Application/Bootstrap/DependencyInjection.cs
using Skywithin.AutoRegister.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddDemoApplication(
        this IServiceCollection services)
    {
        services
            .AddDemoDomain()  // Register Domain layer
            .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
        
        return services;
    }
}
```

### Infrastructure Layer

```csharp
// Demo.Infrastructure/Persistence/UserDataProvider.cs
using Skywithin.AutoRegister.DI;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserDataProvider : IUserDataProvider
{
    private readonly DbContext _dbContext;

    public UserDataProvider(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> GetUserAsync(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email
        };
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        return user;
    }
}

// Demo.Infrastructure/Bootstrap/DependencyInjection.cs
using Skywithin.AutoRegister.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddDemoInfrastructure(
        this IServiceCollection services)
    {
        services
            .AddDemoApplication()  // Register Application layer
            .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
        
        return services;
    }
}
```

### Web API Layer

```csharp
// Demo.Web.Api/Controllers/UserController.cs
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(
        CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}

// Demo.Web.Api/Bootstrap/DependencyInjection.cs
using Skywithin.AutoRegister.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddSwagger()
            .AddDemoInfrastructure()  // Register all lower layers
            .AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly);
        
        return services;
    }
}

// Demo.Web.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
```

## Different Registration Strategies

### Register as Interface Only

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class EmailService : IEmailService
{
    // Registered as IEmailService only
}
```

### Register as Interface with Explicit Specification

When a class implements multiple interfaces, you can explicitly specify which interface to use:

```csharp
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IEmailService))]
public class EmailService : IDisposable, IEmailService, IAsyncDisposable
{
    // Registered as IEmailService, not IDisposable
    // This is useful when implementing framework interfaces like IDisposable
}
```

### Register as Self Only

```csharp
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class StringHelper
{
    // Registered as StringHelper only (no interface)
}
```

### Register as Both

```csharp
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface | RegisterAs.Self)]
public class CacheService : ICacheService
{
    // Registered as both ICacheService and CacheService
}
```

## Multiple Lifetimes Example

```csharp
// Scoped service (per HTTP request)
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
public class UserService : IUserService { }

// Transient service (new instance every time)
[AutoRegister(Lifetime.Transient, RegisterAs.Self)]
public class EmailValidator { }

// Singleton service (one instance for app lifetime)
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
public class ConfigurationService : IConfigurationService { }
```

## Related Topics

- [Getting Started](Getting-Started.md) - Basic setup and usage
- [Advanced Usage](Advanced-Usage.md) - Complex patterns
- [Best Practices](Best-Practices.md) - Guidelines and recommendations

