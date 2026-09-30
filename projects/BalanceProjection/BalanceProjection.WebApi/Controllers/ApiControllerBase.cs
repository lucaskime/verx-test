using BalanceProjection.Application.Results;
using BalanceProjection.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BalanceProjection.WebApi.Controllers;

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
