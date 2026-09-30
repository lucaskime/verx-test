using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BalanceProjection.Application.Results;
using Microsoft.AspNetCore.WebUtilities;

namespace BalanceProjection.WebApi.Responses;

// Builds the envelope and translates the application's errors into HTTP.
internal static class ApiResponses
{
    public static ApiResponse<T> Success<T>(T data, HttpContext httpContext) =>
        new(true, data, [], TraceId(httpContext));

    public static ApiResponse<T> Failure<T>(IEnumerable<ApiError> errors, HttpContext httpContext) =>
        new(false, default, [.. errors], TraceId(httpContext));

    public static ApiResponse<T> Failure<T>(IEnumerable<Error> errors, HttpContext httpContext) =>
        Failure<T>(errors.Select(ToApiError), httpContext);

    public static ApiError ToApiError(Error error) => new(error.Code, error.Message, ToJsonPath(error.Field));

    // The first error decides: a result carries errors of one kind (e.g. all validation).
    public static int StatusCodeFor(IReadOnlyList<Error> errors) => errors[0].Type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError,
    };

    // For failures raised outside MVC (middleware), where there is no Result to map.
    public static Task WriteFailureAsync(HttpContext httpContext, int statusCode, ApiError error)
    {
        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(Failure<object>([error], httpContext), httpContext.RequestAborted);
    }

    public static ApiError HttpError(int statusCode) =>
        new($"Http.{(HttpStatusCode)statusCode}", ReasonPhrases.GetReasonPhrase(statusCode));

    // "Amount" -> "amount", "$.items[0].Name" -> "items[0].name", "$" -> null: fields as the client wrote them.
    public static string? ToJsonPath(string? field)
    {
        var path = field is null ? null : field.StartsWith("$.") ? field[2..] : field.TrimStart('$');
        return string.IsNullOrEmpty(path)
            ? null
            : string.Join('.', path.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
    }

    private static string TraceId(HttpContext httpContext) => Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
