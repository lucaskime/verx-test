using Microsoft.AspNetCore.Mvc;
using Transaction.Application.Results;
using Transaction.WebApi.Responses;

namespace Transaction.WebApi.Controllers;

// Turns a use case Result into the envelope: same shape on success and failure, status from the error type.
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Respond<T>(Result<T> result) => Respond(result, Ok);

    protected ActionResult<ApiResponse<T>> Respond<T>(Result<T> result, Func<ApiResponse<T>, ActionResult> onSuccess) =>
        result.Match(
            value => onSuccess(ApiResponses.Success(value, HttpContext)),
            errors => StatusCode(ApiResponses.StatusCodeFor(errors), ApiResponses.Failure<T>(errors, HttpContext)));
}
