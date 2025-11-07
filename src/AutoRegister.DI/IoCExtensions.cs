using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Skywithin.AutoRegister.DI;

/// <summary>
/// Register services with [AutoRegister] attribute using reflection.
/// </summary>
public static class IoCExtensions
{
    /// <summary>
    /// Register services with [AutoRegister] attribute using reflection from specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for auto-registration attributes.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Should be called as early as possible in the startup process before services are resolved.
    /// This method provides better performance by limiting the scope of assembly scanning.
    /// </remarks>
    public static IServiceCollection AddAutoRegisteredServicesFromAssembly(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        return AddAutoRegisteredServicesFromAssembly(services, logger: null, assemblies);
    }

    /// <summary>
    /// Register services with [AutoRegister] attribute using reflection from specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="logger">Optional callback for logging registration information. If null, console output will be used.</param>
    /// <param name="assemblies">The assemblies to scan for auto-registration attributes.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Should be called as early as possible in the startup process before services are resolved.
    /// This method provides better performance by limiting the scope of assembly scanning.
    /// </remarks>
    public static IServiceCollection AddAutoRegisteredServicesFromAssembly(
        this IServiceCollection services,
        Action<string>? logger,
        params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be specified.", nameof(assemblies));
        }

        foreach (var assembly in assemblies)
        {
            LogMessage(logger, $"Scanning {assembly.GetName().Name} ({assembly.GetName().Version}) assembly for auto-registrations...", LogLevel.Info);
        }

        var registrations = GetTypesToRegister(assemblies, logger).ToList();

        foreach (var registration in registrations)
        {
            RegisterService(services, registration, logger);
        }

        LogMessage(logger, $"Auto-registration completed. Registered {registrations.Count} service(s).", LogLevel.Info);

        return services;
    }

    private static void RegisterService(
        IServiceCollection services,
        ServiceRegistration registration,
        Action<string>? logger)
    {
        if (registration.RegisterAs.HasFlag(RegisterAs.Self))
        {
            LogMessage(logger, $"Registering [{registration.Implementation.FullName}] as [self] with [{registration.ServiceLifetime}] lifetime.", LogLevel.Success);

            AddServiceBasedOnLifetime(services, registration.Implementation,
                registration.Implementation, registration.ServiceLifetime);
        }

        if (registration.RegisterAs.HasFlag(RegisterAs.Interface))
        {
            if (registration.ServiceInterface == null)
            {
                LogMessage(logger, $"WARNING! [{registration.Implementation.FullName}] has RegisterAs.{RegisterAs.Interface} flag but no interface to register. Skipping interface registration.", LogLevel.Warning);
                return;
            }

            LogMessage(logger, $"Registering [{registration.Implementation.FullName}] as [{registration.ServiceInterface.FullName}] with [{registration.ServiceLifetime}] lifetime.", LogLevel.Success);

            AddServiceBasedOnLifetime(services, registration.ServiceInterface,
                registration.Implementation, registration.ServiceLifetime);
        }
    }

    private static IEnumerable<ServiceRegistration> GetTypesToRegister(Assembly[] assemblies, Action<string>? logger)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.IsDefined(typeof(AutoRegisterAttribute), inherit: false) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .Select(type => TryCreateRegistration(type, logger))
            .Where(registration => registration != null)
            .Cast<ServiceRegistration>();
    }

    private static ServiceRegistration? TryCreateRegistration(Type type, Action<string>? logger)
    {
        var attr = type.GetCustomAttribute<AutoRegisterAttribute>();

        if (attr == null)
        {
            // This shouldn't happen due to IsDefined check, but being defensive
            return null;
        }

        // Validate RegisterAs is not empty
        if (attr.RegisterAs == 0)
        {
            LogMessage(logger, $"WARNING! [{type.FullName}] has RegisterAs set to None (0). Skipping registration.", LogLevel.Warning);
            return null;
        }

        Type? serviceInterface = null;

        // If RegisterAs includes Interface flag, we need to determine which interface to use
        if (attr.RegisterAs.HasFlag(RegisterAs.Interface))
        {
            if (attr.ServiceInterface != null)
            {
                // Validate that the specified type is an interface
                if (!attr.ServiceInterface.IsInterface)
                {
                    LogMessage(logger, $"ERROR! [{type.FullName}] specifies [{attr.ServiceInterface.FullName}] as ServiceInterface, but it is not an interface. Skipping registration.", LogLevel.Error);
                    return null;
                }

                // Validate that the type implements the specified interface
                if (!attr.ServiceInterface.IsAssignableFrom(type))
                {
                    LogMessage(logger, $"ERROR! [{type.FullName}] does not implement the specified interface [{attr.ServiceInterface.FullName}]. Skipping registration.", LogLevel.Error);
                    return null;
                }

                serviceInterface = attr.ServiceInterface;
            }
            else
            {
                // Fall back to first suitable interface
                serviceInterface = GetSuitableInterface(type);

                if (serviceInterface == null)
                {
                    LogMessage(logger, $"WARNING! [{type.FullName}] has RegisterAs.{RegisterAs.Interface} flag but implements no suitable interfaces. Skipping interface registration.", LogLevel.Warning);
                    // Don't return null - still register as Self if that flag is set
                }
            }
        }

        return new ServiceRegistration(
            attr.ServiceLifetime,
            attr.RegisterAs,
            serviceInterface,
            type);
    }

    private static Type? GetSuitableInterface(Type type)
    {
        var interfaces = type.GetInterfaces();

        // Filter out common framework interfaces that are typically not meant for DI registration
        var suitableInterfaces = interfaces
            .Where(i =>
                i != typeof(IDisposable) &&
                i != typeof(IAsyncDisposable) &&
                i.Namespace?.StartsWith("System.") != true)
            .ToList();

        if (suitableInterfaces.Count == 0)
        {
            // If no suitable interfaces found, fall back to first interface if any exist
            return interfaces.FirstOrDefault();
        }

        // Return the first suitable interface
        return suitableInterfaces.First();
    }

    private static void AddServiceBasedOnLifetime(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        Lifetime lifetime)
    {
        switch (lifetime)
        {
            case Lifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            case Lifetime.Transient:
                services.AddTransient(serviceType, implementationType);
                break;
            case Lifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime specified.");
        }
    }

    private static void LogMessage(Action<string>? logger, string message, LogLevel level)
    {
        if (logger != null)
        {
            logger(message);
        }
        else
        {
            // Fallback to console output
            switch (level)
            {
                case LogLevel.Error:
                    ConsoleHelper.PrintError(message);
                    break;
                case LogLevel.Warning:
                    ConsoleHelper.PrintWarning(message);
                    break;
                case LogLevel.Success:
                    ConsoleHelper.PrintSuccess(message);
                    break;
                case LogLevel.Info:
                default:
                    ConsoleHelper.PrintInfo(message);
                    break;
            }
        }
    }

    private enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    private record ServiceRegistration(
        Lifetime ServiceLifetime,
        RegisterAs RegisterAs,
        Type? ServiceInterface,
        Type Implementation);
}