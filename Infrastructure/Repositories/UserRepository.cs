using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> LoginUser(string email, string password)
    {
        return await context.Users
            .AnyAsync(x => x.Email == email && x.HashPassword == password);
    }

    public async Task<User?> GetUser(string email)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUser(Guid userId)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
    

    public async Task<bool> ChangeDisplayName(string email, string newName)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
            return false;
        user.Name = newName;
        return true;
    }
}