using BalanceProjection.Application.Reports;
using BalanceProjection.Domain.Reports;
using BalanceProjection.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalanceProjection.WebApi.Controllers;

[Authorize]
[Route("api/balances")]
public sealed class BalancesController : ApiControllerBase
{
    /// <summary>Get the consolidated balance of one account</summary>
    /// <remarks>
    /// Read side: reflects transactions already projected, which may lag behind the write side (eventual consistency).
    /// </remarks>
    [HttpGet("{accountId:int:min(1)}")]
    [ProducesResponseType<ApiResponse<AccountBalanceReport>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<AccountBalanceReport>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResponse<AccountBalanceReport>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AccountBalanceReport>>> Get(
        int accountId,
        [FromServices] GetAccountBalanceHandler handler,
        CancellationToken cancellationToken) =>
        Respond(await handler.HandleAsync(accountId, cancellationToken));

    /// <summary>List consolidated account balances</summary>
    /// <remarks>Returned in stable account order. Same eventual consistency as the single-account query.</remarks>
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<AccountBalanceReport>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<AccountBalanceReport>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<AccountBalanceReport>>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AccountBalanceReport>>>> List(
        [FromQuery] ListAccountBalancesRequest request,
        [FromServices] ListAccountBalancesHandler handler,
        CancellationToken cancellationToken) =>
        Respond(await handler.HandleAsync(request, cancellationToken));
}
