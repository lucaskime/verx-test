using Identity.WebApi.Controllers.Results;

namespace Identity.WebApi.Features.Auth.Register;

public interface IRegisterUserHandler
{
    Task<Result<RegisterUserResponse>> HandleAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}
