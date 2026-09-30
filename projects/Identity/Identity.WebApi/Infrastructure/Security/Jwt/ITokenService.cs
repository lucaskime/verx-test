using Identity.WebApi.Domain.Entities;

namespace Identity.WebApi.Infrastructure.Security.Jwt;

public interface ITokenService
{
    string GenerateAccessToken(User user);
}
