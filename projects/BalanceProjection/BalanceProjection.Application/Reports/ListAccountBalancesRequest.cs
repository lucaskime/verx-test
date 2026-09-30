namespace BalanceProjection.Application.Reports;

/// <summary>Lists consolidated account balances in stable account order. Every field has a default.</summary>
public sealed record ListAccountBalancesRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    /// <summary>1-based page number.</summary>
    /// <example>1</example>
    public int Page { get; init; } = DefaultPage;

    /// <summary>Accounts per page, up to <see cref="MaxPageSize"/>.</summary>
    /// <example>50</example>
    public int PageSize { get; init; } = DefaultPageSize;
}
