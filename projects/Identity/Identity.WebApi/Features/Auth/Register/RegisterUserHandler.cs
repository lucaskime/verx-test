using Identity.WebApi.Controllers.Results;
using Identity.WebApi.Domain.Entities;
using Identity.WebApi.Domain.Errors;
using Identity.WebApi.Domain.Repositories;
using Identity.WebApi.Infrastructure.Security.Cryptography;
using Identity.WebApi.Infrastructure.Security.Jwt;

namespace Identity.WebApi.Features.Auth.Register;

public sealed class RegisterUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRegisterUserHandler
{
    public async Task<Result<RegisterUserResponse>> HandleAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsValidEmail(request.Email))
            return UserErrors.EmailInvalid;

        if (!User.IsStrongPassword(request.Password))
            return UserErrors.PasswordTooWeak;

        var email = User.NormalizeEmail(request.Email);

        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            return UserErrors.EmailAlreadyInUse;

        var user = new User(email, passwordHasher.Hash(request.Password));
        await userRepository.AddAsync(user, cancellationToken);

        var accessToken = tokenService.GenerateAccessToken(user);

        return new RegisterUserResponse(user.Id, user.Email, accessToken);
    }
}
