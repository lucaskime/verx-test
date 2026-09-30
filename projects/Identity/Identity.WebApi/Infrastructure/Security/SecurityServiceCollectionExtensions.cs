using Identity.WebApi.Infrastructure.Security.Cryptography;
using Identity.WebApi.Infrastructure.Security.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Identity.WebApi.Infrastructure.Security;

public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Hash de senha + emissão e validação de JWT com o mesmo par de chaves RSA,
    /// exposto publicamente em /.well-known/jwks.json para os demais microsserviços.
    /// </summary>
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddSingleton<IRsaKeyProvider, FileRsaKeyProvider>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.ConfigureOptions<ConfigureJwtBearerOptions>();

        services.AddAuthorization();

        return services;
    }
}
