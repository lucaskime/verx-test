using System.Text.RegularExpressions;

namespace Identity.WebApi.Domain.Entities;

public sealed partial class User : Entity
{
    public const int MinPasswordLength = 8;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private User()
    {
        // Reservado para materialização pelo EF Core.
    }

    public static User Restore(Guid id, string email, string passwordHash, DateTime createdAtUtc) => new()
    {
        Id = id,
        Email = email,
        PasswordHash = passwordHash,
        CreatedAtUtc = createdAtUtc,
    };

    public User(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailFormatRegex().IsMatch(email.Trim());

    public static bool IsStrongPassword(string? password) =>
        !string.IsNullOrWhiteSpace(password) && password.Length >= MinPasswordLength;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormatRegex();
}
