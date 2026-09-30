using BalanceProjection.Domain.Transactions;

namespace BalanceProjection.Infrastructure.Messaging.Events;

/// <summary>Integration contract emitted by Transaction. Kept local to avoid coupling the services.</summary>
public sealed record BalanceTransactionCreatedEvent(
    Guid Id,
    Guid RawTransactionId,
    TransactionType Type,
    int AccountId,
    decimal Amount,
    DateTimeOffset CreatedAt);
