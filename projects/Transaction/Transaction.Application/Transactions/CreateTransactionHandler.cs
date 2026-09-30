using Transaction.Application.Results;
using Transaction.Domain.Entities;
using Transaction.Domain.Repositories;

namespace Transaction.Application.Transactions;

public sealed class CreateTransactionHandler(ITransactionRepository repository, TimeProvider timeProvider)
{
    /// <param name="idempotencyScope">Who owns the key (the authenticated caller): keys of different callers never collide.</param>
    /// <param name="idempotencyKey">Client-chosen key; a retry with the same key and request returns the original outcome.</param>
    public async Task<Result<CreateTransactionOutcome>> HandleAsync(
        CreateTransactionRequest request, string idempotencyScope, string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var keyValidation = IdempotencyKeys.Validate(idempotencyKey);
        var validation = CreateTransactionValidator.Validate(request);
        List<Error> errors = [.. keyValidation.Errors, .. validation.Errors];
        if (errors.Count > 0)
            return Result.Failure<CreateTransactionOutcome>(errors);

        var requestHash = IdempotencyKeys.Hash(request);

        // Fast path for a retry; the unique index below is what actually guarantees it under concurrency.
        var existing = await repository.FindByIdempotencyKeyAsync(idempotencyScope, idempotencyKey!, cancellationToken);
        if (existing is not null)
            return Replay(existing, requestHash);

        // Validated above: every member is present.
        var transaction = new RawTransaction
        {
            Type = request.Type!.Value,
            AccountId = request.AccountId!.Value,
            Amount = request.Amount!.Value,
            CreatedBy = request.CreatedBy!.Trim(),
            CreatedAt = TruncateToMicroseconds(timeProvider.GetUtcNow()),
            IdempotencyScope = idempotencyScope,
            IdempotencyKey = idempotencyKey!,
            RequestHash = requestHash,
        };

        try
        {
            await repository.AddAsync(transaction, cancellationToken);
        }
        catch (DuplicateIdempotencyKeyException)
        {
            // Lost the race against a concurrent request with the same key: answer from the winner.
            existing = await repository.FindByIdempotencyKeyAsync(idempotencyScope, idempotencyKey!, cancellationToken);
            return Replay(existing!, requestHash);
        }

        return new CreateTransactionOutcome(CreateTransactionResponse.From(transaction), Replayed: false);
    }

    private static Result<CreateTransactionOutcome> Replay(RawTransaction existing, string requestHash) =>
        existing.RequestHash == requestHash
            ? new CreateTransactionOutcome(CreateTransactionResponse.From(existing), Replayed: true)
            : IdempotencyKeys.Reused();

    // PostgreSQL stores microseconds: truncating up front makes a replay identical to the first response.
    private static DateTimeOffset TruncateToMicroseconds(DateTimeOffset value) =>
        new(value.Ticks - value.Ticks % 10, value.Offset);
}
