using Transaction.Domain.Entities;
using Transaction.Domain.Transactions;

namespace Transaction.Infrastructure.Messaging.Events;

/// <summary>Message published to RabbitMQ for each outbox entry.</summary>
public sealed record BalanceTransactionCreatedEvent(
    Guid Id,
    Guid RawTransactionId,
    TransactionType Type,
    int AccountId,
    decimal Amount,
    DateTimeOffset CreatedAt)
{
    public static BalanceTransactionCreatedEvent From(BalanceTransaction entry) => new(
        entry.Id,
        entry.RawTransactionId,
        entry.Type,
        entry.AccountId,
        entry.Amount,
        entry.CreatedAt);
}
