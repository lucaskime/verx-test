using Identity.WebApi.Controllers.Results;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApi.Controllers.Api;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<TValue>(Result<TValue> result)
    {
        return result.IsSuccess
            ? Ok(ApiResponse<TValue>.Ok(result.Value))
            : MapError<TValue>(result.Error);
    }

    protected IActionResult FromResult(Result result)
    {
        return result.IsSuccess
            ? Ok(ApiResponse<object>.Ok(null))
            : MapError<object>(result.Error);
    }

    private IActionResult MapError<TValue>(Error error)
    {
        var response = ApiResponse<TValue>.Fail(error);

        return error.Type switch
        {
            ErrorType.Validation => BadRequest(response),
            ErrorType.NotFound => NotFound(response),
            ErrorType.Conflict => Conflict(response),
            ErrorType.Unauthorized => Unauthorized(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response),
        };
    }
}
