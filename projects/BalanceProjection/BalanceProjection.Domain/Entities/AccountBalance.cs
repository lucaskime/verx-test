namespace BalanceProjection.Domain.Entities;

/// <summary>Read model consolidated per account and optimized for reporting.</summary>
public sealed class AccountBalance
{
    public int AccountId { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal TotalDebits { get; set; }
    public long TransactionCount { get; set; }
    public DateTimeOffset LastTransactionAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
