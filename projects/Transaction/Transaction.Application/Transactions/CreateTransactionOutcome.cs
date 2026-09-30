namespace Transaction.Application.Transactions;

/// <summary>What the use case produced, and whether it came from an earlier request with the same Idempotency-Key.</summary>
/// <param name="Transaction">The recorded transaction.</param>
/// <param name="Replayed">True when nothing was written now: the original result is being returned again.</param>
public sealed record CreateTransactionOutcome(CreateTransactionResponse Transaction, bool Replayed);
