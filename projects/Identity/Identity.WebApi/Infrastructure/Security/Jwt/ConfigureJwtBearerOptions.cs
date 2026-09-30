using Microsoft.AspNetCore.Authentication.JwtBearer;
using Identity.WebApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.WebApi.Infrastructure.Security.Jwt;

/// <summary>
/// Liga o JwtBearer ao mesmo par de chaves usado para assinar os tokens (<see cref="IRsaKeyProvider"/>),
/// resolvido via DI para evitar a criação de um ServiceProvider auxiliar durante o startup.
/// </summary>
public sealed class ConfigureJwtBearerOptions(IRsaKeyProvider keyProvider, IOptions<JwtOptions> jwtOptions)
    : IPostConfigureOptions<JwtBearerOptions>
{
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var jwt = jwtOptions.Value;

        // Mantém os nomes de claim originais do token (ex.: "sub"), sem o remapeamento
        // padrão do ASP.NET para tipos legados como ClaimTypes.NameIdentifier.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = keyProvider.GetSigningKey(),
        };
    }
}
