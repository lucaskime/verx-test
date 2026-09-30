namespace BalanceProjection.Domain.Entities;

/// <summary>Durable inbox entry. Its primary key makes event handling idempotent.</summary>
public sealed class ProcessedTransaction
{
    public Guid TransactionId { get; set; }
    public Guid RawTransactionId { get; set; }
    public int AccountId { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
