namespace Shared.Models.DTO.Request;

public sealed class RegisterRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? Name { get; init; } = null!;
}