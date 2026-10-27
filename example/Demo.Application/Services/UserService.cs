using Skywithin.AutoRegister.DI;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.SharedKernel.Services;

namespace Demo.Application.Services;

// Depends on abstractions from two lower layers; both are registered by those layers' scans.
[AutoRegister(Lifetime.Scoped, RegisterAs.Interface)]
internal sealed class UserService(
    IUserDataProvider userDataProvider,
    IClockService clock) : IUserService
{
    public User Create(string name, string email)
    {
        var user = new User(Guid.NewGuid(), name, email, clock.UtcNow);
        userDataProvider.Add(user);
        return user;
    }

    public User? GetById(Guid id) => userDataProvider.Find(id);
}
