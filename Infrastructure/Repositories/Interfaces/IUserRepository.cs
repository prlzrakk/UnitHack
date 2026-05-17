

using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User?> RegisterUser(string email, string name, string hashPassword);
    public Task<bool> LoginUser(string email, string password);
    public Task<User?> GetUser(string email);
    public Task<User?> GetUser(Guid userId);
    public Task<List<User>> SearchUsers(string query, int limit, CancellationToken cancellationToken);
    public Task<bool> ChangeDisplayName(string email, string newName);
}
