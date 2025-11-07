namespace Skywithin.AutoRegister.DI;

public enum Lifetime
{
    Scoped,
    Transient,
    Singleton,
}

[Flags]
public enum RegisterAs
{
    Interface = 1 << 0,
    Self = 1 << 1,
}

/// <summary>
/// Class marked with this attribute will be auto-registered 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoRegisterAttribute : Attribute
{
    public AutoRegisterAttribute(
        Lifetime serviceLifetime,
        RegisterAs registerAs)
    {
        ServiceLifetime = serviceLifetime;
        RegisterAs = registerAs;
        ServiceInterface = null;
    }

    public AutoRegisterAttribute(
        Lifetime serviceLifetime,
        RegisterAs registerAs,
        Type serviceInterface)
    {
        ServiceLifetime = serviceLifetime;
        RegisterAs = registerAs;
        ServiceInterface = serviceInterface;
    }

    /// <summary>
    /// The service lifetime for the registration.
    /// </summary>
    public Lifetime ServiceLifetime { get; }

    /// <summary>
    /// Defines whether to register the class as itself, its interface, or both.
    /// </summary>
    public RegisterAs RegisterAs { get; }

    /// <summary>
    /// Optional: Specifies which interface to register the service as.
    /// If not specified, the first interface implemented by the class will be used.
    /// </summary>
    public Type? ServiceInterface { get; }
}
