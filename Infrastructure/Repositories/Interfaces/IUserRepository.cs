

using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IUserRepository
{
    public Task<User?> RegisterUser(string email, string name, string hashPassword);
    public Task<bool> LoginUser(string email, string password);
    public Task<User?> GetUser(string email);
    public Task<User?> GetUser(int userId);
    public Task<bool> ChangeDisplayName(string email, string newName);
}
