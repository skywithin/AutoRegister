using Demo.Domain.Models;

namespace Demo.Domain.Abstractions;

public interface IUserDataProvider
{
    void Add(User user);

    User? Find(Guid id);
}
