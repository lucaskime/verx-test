using Transaction.Domain.Entities.Abstractions;
using Transaction.Domain.Transactions;

namespace Transaction.Domain.Entities;

public class BalanceTransaction : IEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid RawTransactionId { get; set; }
    public TransactionType Type { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }

    // Broker ACK: the event reached the topic. Says nothing about the balance.
    public DateTimeOffset? PublishedAt { get; set; }

    // Business confirmation: the Balance Projection applied the entry.
    public DateTimeOffset? ProcessedAt { get; set; }

    public string CreatedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    // One balance entry per raw transaction, carrying the fields the balance needs.
    public static BalanceTransaction From(RawTransaction raw) => new()
    {
        RawTransactionId = raw.Id,
        Type = raw.Type,
        AccountId = raw.AccountId,
        Amount = raw.Amount,
        CreatedBy = raw.CreatedBy,
        CreatedAt = raw.CreatedAt,
    };
}
