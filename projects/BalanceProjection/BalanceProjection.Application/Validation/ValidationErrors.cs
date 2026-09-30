using BalanceProjection.Application.Results;

namespace BalanceProjection.Application.Validation;

public static class ValidationErrors
{
    // Shared by every input error, whether caught here or by the host (e.g. model binding).
    public const string InvalidCode = "Request.Invalid";

    public static Error Invalid(string message, string? field = null) =>
        Error.Validation(InvalidCode, message, string.IsNullOrEmpty(field) ? null : field);
}
