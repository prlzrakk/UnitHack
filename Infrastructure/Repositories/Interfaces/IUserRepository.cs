using Shared.Models.Entities;

namespace Infrastructure.Interfaces;

public interface IUserRepository
{
    public Task<User?> RegisterUser(string email, string hashPassword);
    public Task<bool> LoginUser(string email, string hashPassword);
    public Task<User?> GetUser(string email);
    public Task<User?> GetUser(int userTaskId);
    public Task<bool> ChangeDisplayName(string email, string newName);
}
