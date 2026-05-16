using Infrastructure.Db;
using Infrastructure.Repositories.Interfaces;
using User = Client.Models.Entities.User;

namespace Infrastructure.Repositories;

public class UserRepository(DatabaseContext context) : IUserRepository
{
    private DatabaseContext _context = context;

    public Task<User?> RegisterUser(string email, string hashPassword)
    {
        throw new NotImplementedException();
    }

    Task<User?> IUserRepository.RegisterUser(string email,string name, string hashPassword)
    {
        return RegisterUser(email, hashPassword);
    }

    public Task<User?> RegisterUser(string email, string name, string hashPassword)
    {
        throw new NotImplementedException();
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

    public Task<User?> GetUser(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeDisplayName(string email, string newName)
    {
        throw new NotImplementedException();
    }
}