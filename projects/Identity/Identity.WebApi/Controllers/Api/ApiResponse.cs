using Identity.WebApi.Controllers.Results;

namespace Identity.WebApi.Controllers.Api;

public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiResponse<T> Ok(T? data) => new(true, data, null);

    public static ApiResponse<T> Fail(Error error) => new(false, default, new ApiError(error.Code, error.Message));
}
