using Infrastructure.Entities;
using Infrastructure.Constants;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class UserRepositoryMock(IPasswordHasher hasher) : IUserRepository
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, MockUser> Users = new(StringComparer.OrdinalIgnoreCase);

    public Task<User?> RegisterUser(string email, string name, string hashPassword)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (Sync)
        {
            SeedDefaultUser();

            if (Users.ContainsKey(normalizedEmail))
                return Task.FromResult<User?>(null);

            var user = new MockUser
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                Name = string.IsNullOrWhiteSpace(name)
                    ? CreateDefaultName(normalizedEmail)
                    : name.Trim(),
                HashPassword = hashPassword
            };

            Users.Add(normalizedEmail, user);
            return Task.FromResult<User?>(ToUser(user));
        }
    }

    public Task<bool> LoginUser(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (Sync)
        {
            SeedDefaultUser();

            var isValid = Users.TryGetValue(normalizedEmail, out var user)
                          && hasher.Verify(password, user.HashPassword);
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

    public Task<User?> GetUser(Guid userId)
    {
        lock (Sync)
        {
            SeedDefaultUser();

            if (userId == Guid.Empty)
                return Task.FromResult<User?>(null);

            var user = Users.Values.FirstOrDefault(u => u.Id == userId);
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
        const string email = DevSeedDefaults.UserEmail;
        if (Users.ContainsKey(email))
            return;

        Users.Add(email, new MockUser
        {
            Id = DevSeedDefaults.UserId,
            Email = email,
            Name = DevSeedDefaults.UserName,
            HashPassword = hasher.Hash(DevSeedDefaults.UserPassword)
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
        Id = user.Id,
        Email = user.Email,
        Name = user.Name
    };

    private sealed class MockUser
    {
        public required Guid Id { get; init; }
        public required string Email { get; init; }
        public required string Name { get; set; }
        public required string HashPassword { get; init; }
    }
}
