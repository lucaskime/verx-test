using BalanceProjection.Application.Results;

namespace BalanceProjection.Application.Reports;

// Business errors for the Reports use cases; codes are stable for clients to branch on.
internal static class ReportErrors
{
    public static Error AccountNotFound(int accountId) =>
        Error.NotFound("Account.NotFound", $"Account '{accountId}' was not found.");
}
