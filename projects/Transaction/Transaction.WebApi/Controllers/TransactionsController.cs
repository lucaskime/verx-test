using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transaction.Application.Transactions;
using Transaction.WebApi.Responses;

namespace Transaction.WebApi.Controllers;

[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController : ApiControllerBase
{
    /// <summary>Record a transaction</summary>
    /// <remarks>
    /// 201 confirms the transaction and its outbox entry were persisted, not that the balance was updated.
    /// No Location: this service only records; queries belong to the read side (balance/statement).
    ///
    /// Idempotent: the <c>Idempotency-Key</c> header is required (client-generated, e.g. a UUID, up to 255 visible ASCII characters).
    /// Retrying with the same key and body returns the original 201 with <c>Idempotent-Replayed: true</c> and records nothing again.
    /// The same key with a different body is 422 (<c>Idempotency.KeyReused</c>). Keys are scoped to the authenticated caller.
    /// </remarks>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ApiResponse<CreateTransactionResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<CreateTransactionResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<CreateTransactionResponse>>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ApiResponse<CreateTransactionResponse>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateTransactionResponse>>> Create(
        [FromBody] CreateTransactionRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromServices] CreateTransactionHandler handler, CancellationToken cancellationToken)
    {
        // The key belongs to the caller: the same key from two callers is two different intents.
        var caller = User.FindFirstValue("sub");
        if (caller is null)
            return Unauthorized();

        var result = await handler.HandleAsync(request, caller, idempotencyKey, cancellationToken);
        return Respond(result.Map(outcome => outcome.Transaction), response =>
        {
            if (result.Value.Replayed)
                Response.Headers["Idempotent-Replayed"] = "true";
            return StatusCode(StatusCodes.Status201Created, response);
        });
    }
}
