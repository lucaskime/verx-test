using Transaction.Domain.Entities.Abstractions;
using Transaction.Domain.Transactions;

namespace Transaction.Domain.Entities;

public class RawTransaction : IEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public TransactionType Type { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    // Idempotency: (scope, key) identifies the client's intent; the hash tells a retry from a key reused for other data.
    public string IdempotencyScope { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public string RequestHash { get; set; } = null!;
}
