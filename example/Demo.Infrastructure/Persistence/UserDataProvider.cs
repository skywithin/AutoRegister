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

    public void Add(User user) => _users[user.Id] = user;

    public User? Find(Guid id) => _users.GetValueOrDefault(id);
}
