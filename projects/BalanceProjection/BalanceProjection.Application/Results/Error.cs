namespace BalanceProjection.Application.Results;

/// <summary>
/// Expected failure of an operation. <see cref="Code"/> is stable for clients to branch on;
/// <see cref="Message"/> is human-readable; <see cref="Field"/> points to the offending input, if any.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type, string? Field = null)
{
    public static Error Validation(string code, string message, string? field = null) =>
        new(code, message, ErrorType.Validation, field);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}
