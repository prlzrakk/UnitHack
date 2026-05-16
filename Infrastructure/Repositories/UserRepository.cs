using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories;

public class UserRepository(DatabaseContext context) : IUserRepository
{
    public async Task<User?> RegisterUser(string email, string name, string hashPassword)
    {
        if (context.Users.Any(u => u.Email == email)) return null;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            HashPassword = hashPassword
        };
        await context.Users.AddAsync(user);
        return user;
    }

    public Task<bool> LoginUser(string email, string password)
    {
        throw new NotImplementedException();
    }

    Task<User?> IUserRepository.GetUser(string email)
    {
        return GetUser(email);
    }

    public Task<User?> GetUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUser(string email)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUser(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeDisplayName(string email, string newName)
    {
        throw new NotImplementedException();
    }
}