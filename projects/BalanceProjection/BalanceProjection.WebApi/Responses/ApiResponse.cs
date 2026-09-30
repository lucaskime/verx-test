namespace BalanceProjection.WebApi.Responses;

/// <summary>
/// Single envelope for every API response, success or failure, so clients parse one shape:
/// <c>{ success, data, errors, traceId }</c>. On success <see cref="Errors"/> is empty;
/// on failure <see cref="Data"/> is null. The HTTP status still carries the outcome class.
/// </summary>
public sealed record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<ApiError> Errors, string TraceId);

public sealed record ApiError(string Code, string Message, string? Field = null);
