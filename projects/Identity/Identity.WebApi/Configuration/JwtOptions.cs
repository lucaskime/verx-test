using System.ComponentModel.DataAnnotations;

namespace Identity.WebApi.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 60;

    [Required]
    public string SigningKeyPath { get; init; } = "keys/identity-signing-key.json";
}
