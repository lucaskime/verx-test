using Identity.WebApi.Controllers.Results;

namespace Identity.WebApi.Features.Auth.Login;

public interface ILoginUserHandler
{
    Task<Result<LoginUserResponse>> HandleAsync(LoginUserRequest request, CancellationToken cancellationToken);
}
