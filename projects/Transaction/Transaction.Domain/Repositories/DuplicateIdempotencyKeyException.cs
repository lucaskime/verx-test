namespace Transaction.Domain.Repositories;

/// <summary>The (scope, key) pair was already recorded, typically by a concurrent request with the same Idempotency-Key.</summary>
public sealed class DuplicateIdempotencyKeyException(Exception innerException)
    : Exception("The idempotency key was already used.", innerException);
