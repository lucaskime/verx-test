using Microsoft.IdentityModel.Tokens;

namespace Identity.WebApi.Infrastructure.Security.Jwt;

public interface IRsaKeyProvider
{
    string KeyId { get; }

    RsaSecurityKey GetSigningKey();

    JsonWebKeySet GetPublicJwks();
}
