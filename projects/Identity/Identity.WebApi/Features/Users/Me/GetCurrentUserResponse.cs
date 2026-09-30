namespace Identity.WebApi.Features.Users.Me;

public sealed record GetCurrentUserResponse(Guid UserId, string Email);
