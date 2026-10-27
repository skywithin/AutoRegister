using Demo.Domain.Models;

namespace Demo.Application.Services;

public interface IUserService
{
    User Create(string name, string email);

    User? GetById(Guid id);
}
