using Client.Models.Entities;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class UserRepositoryMock(IPasswordHasher hasher) : IUserRepository
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, MockUser> Users = new(StringComparer.OrdinalIgnoreCase);

    public Task<User?> RegisterUser(string email, string hashPassword)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (Sync)
        {
            SeedDefaultUser();

            if (Users.ContainsKey(normalizedEmail))
                return Task.FromResult<User?>(null);

            var user = new MockUser
            {
                Email = normalizedEmail,
                Name = CreateDefaultName(normalizedEmail),
                HashPassword = hashPassword
            };

            Users.Add(normalizedEmail, user);
            return Task.FromResult<User?>(ToUser(user));
        }
    }

    public Task<bool> LoginUser(string email, string hashPassword)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (Sync)
        {
            SeedDefaultUser();

            var isValid = Users.TryGetValue(normalizedEmail, out var user)
                          && hasher.Verify(hashPassword, user.HashPassword);
            return Task.FromResult(isValid);
        }
    }

    public Task<User?> GetUser(string email)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (Sync)
        {
            SeedDefaultUser();

            return Task.FromResult(
                Users.TryGetValue(normalizedEmail, out var user)
                    ? ToUser(user)
                    : null);
        }
    }

    public Task<User?> GetUser(int userTaskId)
    {
        lock (Sync)
        {
            SeedDefaultUser();

            if (userTaskId <= 0)
                return Task.FromResult<User?>(null);

            var user = Users.Values.ElementAtOrDefault(userTaskId - 1);
            return Task.FromResult(user is null ? null : ToUser(user));
        }
    }

    public Task<bool> ChangeDisplayName(string email, string newName)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (Sync)
        {
            SeedDefaultUser();

            if (!Users.TryGetValue(normalizedEmail, out var user))
                return Task.FromResult(false);

            user.Name = newName.Trim();
            return Task.FromResult(true);
        }
    }

    private void SeedDefaultUser()
    {
        const string email = "test@example.com";
        if (Users.ContainsKey(email))
            return;

        Users.Add(email, new MockUser
        {
            Email = email,
            Name = "Test User",
            HashPassword = hasher.Hash("password")
        });
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string CreateDefaultName(string email)
    {
        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email[..atIndex] : email;
    }

    private static User ToUser(MockUser user) => new()
    {
        Email = user.Email,
        Name = user.Name
    };

    private sealed class MockUser
    {
        public required string Email { get; init; }
        public required string Name { get; set; }
        public required string HashPassword { get; init; }
    }
}
