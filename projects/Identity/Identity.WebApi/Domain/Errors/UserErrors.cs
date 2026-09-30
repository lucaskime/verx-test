using Identity.WebApi.Controllers.Results;
using Identity.WebApi.Domain.Entities;

namespace Identity.WebApi.Domain.Errors;

public static class UserErrors
{
    public static readonly Error EmailInvalid = Error.Validation("User.EmailInvalid", "Email ausente ou em formato inválido.");

    public static readonly Error EmailAlreadyInUse = Error.Conflict("User.EmailAlreadyInUse", "Já existe uma conta com este email.");

    public static readonly Error PasswordTooWeak = Error.Validation("User.PasswordTooWeak", $"A senha deve ter pelo menos {User.MinPasswordLength} caracteres.");

    public static readonly Error InvalidCredentials = Error.Unauthorized("User.InvalidCredentials", "Email ou senha inválidos.");

    public static readonly Error NotFound = Error.NotFound("User.NotFound", "Usuário não encontrado.");
}
