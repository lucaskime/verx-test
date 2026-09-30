namespace BalanceProjection.Infrastructure.Messaging.Events;

/// <summary>Business ACK consumed by Transaction to fill its own ProcessedAt field.</summary>
public sealed record BalanceTransactionProcessedEvent(
    Guid TransactionId,
    Guid RawTransactionId,
    int AccountId,
    DateTimeOffset ProcessedAt);
