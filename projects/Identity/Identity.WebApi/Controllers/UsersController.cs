using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.WebApi.Controllers.Api;
using Identity.WebApi.Domain.Errors;
using Identity.WebApi.Features.Users.Me;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApi.Controllers;

[Route("api/users")]
[Authorize]
public sealed class UsersController(IGetCurrentUserHandler getCurrentUserHandler) : ApiControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (subject is null || !Guid.TryParse(subject, out var userId))
            return Unauthorized(ApiResponse<object>.Fail(UserErrors.InvalidCredentials));

        var result = await getCurrentUserHandler.HandleAsync(userId, cancellationToken);
        return FromResult(result);
    }
}
