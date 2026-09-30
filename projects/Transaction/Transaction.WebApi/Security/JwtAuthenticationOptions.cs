using System.ComponentModel.DataAnnotations;

namespace Transaction.WebApi.Security;

/// <summary>Where and how to validate tokens issued by the Identity service.</summary>
public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>Identity's public JWKS endpoint (/.well-known/jwks.json), consumed to obtain the signing keys.</summary>
    [Required, Url]
    public string JwksUrl { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>Set to false only for local development against Identity over plain HTTP.</summary>
    public bool RequireHttpsMetadata { get; init; } = true;
}
