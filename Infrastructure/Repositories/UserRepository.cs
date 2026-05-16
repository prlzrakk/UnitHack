using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(DatabaseContext context) : IUserRepository
{
    public async Task<User?> RegisterUser(string email, string name, string hashPassword)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (await context.Users.AnyAsync(u => u.Email == normalizedEmail))
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = normalizedEmail,
            HashPassword = hashPassword
        };

        await context.Users.AddAsync(user);
        return user;
    }

    public async Task<bool> LoginUser(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);

        return await context.Users
            .AnyAsync(x => x.Email == normalizedEmail && x.HashPassword == password);
    }

    public async Task<User?> GetUser(string email)
    {
        var normalizedEmail = NormalizeEmail(email);

        return await context.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);
    }

    public async Task<User?> GetUser(Guid userId)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<bool> ChangeDisplayName(string email, string newName)
    {
        var normalizedEmail = NormalizeEmail(email);

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user is null)
            return false;

        user.Name = newName;
        return true;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
