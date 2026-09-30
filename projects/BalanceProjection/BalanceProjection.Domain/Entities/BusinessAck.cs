namespace BalanceProjection.Domain.Entities;

/// <summary>Transactional outbox entry for the business-processing acknowledgement.</summary>
public sealed class BusinessAck
{
    public Guid TransactionId { get; set; }
    public Guid RawTransactionId { get; set; }
    public int AccountId { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
