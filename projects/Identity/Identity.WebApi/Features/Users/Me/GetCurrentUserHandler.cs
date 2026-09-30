using Identity.WebApi.Controllers.Results;
using Identity.WebApi.Domain.Errors;
using Identity.WebApi.Domain.Repositories;

namespace Identity.WebApi.Features.Users.Me;

public sealed class GetCurrentUserHandler(IUserRepository userRepository) : IGetCurrentUserHandler
{
    public async Task<Result<GetCurrentUserResponse>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        return user is null
            ? UserErrors.NotFound
            : new GetCurrentUserResponse(user.Id, user.Email);
    }
}
