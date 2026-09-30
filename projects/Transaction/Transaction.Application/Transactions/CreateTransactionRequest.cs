using System.Text.Json.Serialization;
using Transaction.Domain.Transactions;

namespace Transaction.Application.Transactions;

/// <summary>Records a transaction. Every field is mandatory; rules are in <see cref="CreateTransactionValidator"/>.</summary>
/// <remarks>
/// Members are nullable so an absent field is reported by name as required instead of
/// silently becoming a default (Guid.Empty, 0). Unmapped fields are rejected, not ignored.
/// </remarks>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateTransactionRequest
{
    /// <summary>Required. Kind of entry: <c>Debit</c> or <c>Credit</c>.</summary>
    /// <example>Debit</example>
    public TransactionType? Type { get; init; }

    /// <summary>Required. Account the entry belongs to.</summary>
    /// <example>1</example>
    public int? AccountId { get; init; }

    /// <summary>Required. Positive value with at most two decimal places, fitting decimal(18,2).</summary>
    /// <example>12.34</example>
    public decimal? Amount { get; init; }

    /// <summary>Required. Who records the entry, up to 100 characters.</summary>
    /// <example>client</example>
    public string? CreatedBy { get; init; }
}
