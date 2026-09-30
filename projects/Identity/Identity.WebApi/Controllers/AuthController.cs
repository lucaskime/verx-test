using Identity.WebApi.Controllers.Api;
using Identity.WebApi.Features.Auth.Login;
using Identity.WebApi.Features.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApi.Controllers;

[Route("api/auth")]
public sealed class AuthController(
    IRegisterUserHandler registerUserHandler,
    ILoginUserHandler loginUserHandler) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var result = await registerUserHandler.HandleAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var result = await loginUserHandler.HandleAsync(request, cancellationToken);
        return FromResult(result);
    }
}
