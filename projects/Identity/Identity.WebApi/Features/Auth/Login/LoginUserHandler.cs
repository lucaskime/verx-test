using Identity.WebApi.Controllers.Results;
using Identity.WebApi.Domain.Entities;
using Identity.WebApi.Domain.Errors;
using Identity.WebApi.Domain.Repositories;
using Identity.WebApi.Infrastructure.Security.Cryptography;
using Identity.WebApi.Infrastructure.Security.Jwt;

namespace Identity.WebApi.Features.Auth.Login;

public sealed class LoginUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : ILoginUserHandler
{
    public async Task<Result<LoginUserResponse>> HandleAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsValidEmail(request.Email))
            return UserErrors.InvalidCredentials;

        var user = await userRepository.GetByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return UserErrors.InvalidCredentials;

        var accessToken = tokenService.GenerateAccessToken(user);

        return new LoginUserResponse(user.Id, user.Email, accessToken);
    }
}
