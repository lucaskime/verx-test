namespace BalanceProjection.Application.Results;

// Error category; the presentation layer maps it to its own protocol (e.g. HTTP status).
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
    Unexpected,
}
