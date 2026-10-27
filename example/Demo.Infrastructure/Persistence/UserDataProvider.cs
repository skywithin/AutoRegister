using System.Collections.Concurrent;
using Skywithin.AutoRegister.DI;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;

namespace Demo.Infrastructure.Persistence;

// Singleton so the in-memory data outlives a single scope; the concurrent
// dictionary keeps it safe to share across scopes and threads.
[AutoRegister(Lifetime.Singleton, RegisterAs.Interface)]
internal sealed class UserDataProvider : IUserDataProvider
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public UserDataProvider()
    {
        Seed(new User
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1-555-0123",
            DateOfBirth = new DateTime(1990, 5, 15),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        });

        Seed(new User
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "+1-555-0456",
            DateOfBirth = new DateTime(1985, 8, 22),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        });

        Seed(new User
        {
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob.johnson@example.com",
            PhoneNumber = "+1-555-0789",
            DateOfBirth = new DateTime(1992, 12, 3),
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        });
    }

    public IReadOnlyCollection<User> GetAll() => _users.Values.ToList();

    public User? Find(Guid id) => _users.GetValueOrDefault(id);

    public bool TryAdd(User user) => _users.TryAdd(user.Id, user);

    public bool Remove(Guid id) => _users.TryRemove(id, out _);

    private void Seed(User user)
    {
        user.Id = Guid.NewGuid();
        _users[user.Id] = user;
    }
}
