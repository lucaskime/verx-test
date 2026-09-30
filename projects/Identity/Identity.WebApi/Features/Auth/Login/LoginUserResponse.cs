namespace Identity.WebApi.Features.Auth.Login;

public sealed record LoginUserResponse(Guid UserId, string Email, string AccessToken);
