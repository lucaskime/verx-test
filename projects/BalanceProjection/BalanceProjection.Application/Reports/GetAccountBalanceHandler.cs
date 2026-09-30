using BalanceProjection.Application.Results;
using BalanceProjection.Domain.Reports;
using BalanceProjection.Domain.Repositories;

namespace BalanceProjection.Application.Reports;

public sealed class GetAccountBalanceHandler(IBalanceReportRepository repository)
{
    public async Task<Result<AccountBalanceReport>> HandleAsync(
        int accountId, CancellationToken cancellationToken = default)
    {
        var report = await repository.GetAsync(accountId, cancellationToken);
        return report is null ? ReportErrors.AccountNotFound(accountId) : report;
    }
}
