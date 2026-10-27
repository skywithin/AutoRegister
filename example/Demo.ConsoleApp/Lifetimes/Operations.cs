using Skywithin.AutoRegister.DI;

namespace Demo.ConsoleApp.Lifetimes;

public interface IOperation
{
    Guid Id { get; }
}

public interface ITransientOperation : IOperation;

public interface IScopedOperation : IOperation;

public interface ISingletonOperation : IOperation;

internal abstract class Operation : IOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}

// Each class implements two interfaces (its own and IOperation), so the
// interface to register is named explicitly.

[AutoRegister(Lifetime.Transient, RegisterAs.Interface, typeof(ITransientOperation))]
internal sealed class TransientOperation : Operation, ITransientOperation;

[AutoRegister(Lifetime.Scoped, RegisterAs.Interface, typeof(IScopedOperation))]
internal sealed class ScopedOperation : Operation, IScopedOperation;

[AutoRegister(Lifetime.Singleton, RegisterAs.Interface, typeof(ISingletonOperation))]
internal sealed class SingletonOperation : Operation, ISingletonOperation;
