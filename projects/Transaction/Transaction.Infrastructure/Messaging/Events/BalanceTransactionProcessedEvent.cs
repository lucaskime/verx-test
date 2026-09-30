namespace Transaction.Infrastructure.Messaging.Events;

/// <summary>Business acknowledgement emitted after BalanceProjection commits the read model.</summary>
public sealed record BalanceTransactionProcessedEvent(
    Guid TransactionId,
    Guid RawTransactionId,
    int AccountId,
    DateTimeOffset ProcessedAt);
