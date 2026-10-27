using Demo.Domain.Models;

namespace Demo.Domain.Abstractions;

public interface IUserDataProvider
{
    IReadOnlyCollection<User> GetAll();

    User? Find(Guid id);

    bool TryAdd(User user);

    bool Remove(Guid id);
}
