using Identity.WebApi.Controllers.Results;

namespace Identity.WebApi.Features.Users.Me;

public interface IGetCurrentUserHandler
{
    Task<Result<GetCurrentUserResponse>> HandleAsync(Guid userId, CancellationToken cancellationToken);
}
