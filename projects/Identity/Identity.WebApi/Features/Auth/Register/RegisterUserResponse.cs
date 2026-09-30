namespace Identity.WebApi.Features.Auth.Register;

public sealed record RegisterUserResponse(Guid UserId, string Email, string AccessToken);
