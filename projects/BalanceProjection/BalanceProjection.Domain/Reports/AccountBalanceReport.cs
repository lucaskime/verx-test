namespace BalanceProjection.Domain.Reports;

public sealed record AccountBalanceReport(
    int AccountId,
    decimal Balance,
    decimal TotalCredits,
    decimal TotalDebits,
    long TransactionCount,
    DateTimeOffset LastTransactionAt,
    DateTimeOffset UpdatedAt);
