using Infrastructure.Db;
using Infrastructure.Interfaces;
using Shared.Models.Entities;

namespace Infrastructure.Repositories;

public class UserRepository(DatabaseContext context) : IUserRepository
{
    private DatabaseContext _context = context;

    public Task<User?> RegisterUser(string email, string hashPassword)
    {
        
    }

    public Task<bool> LoginUser(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUser(string email)
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