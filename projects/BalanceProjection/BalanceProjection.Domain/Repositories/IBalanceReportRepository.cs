using BalanceProjection.Domain.Reports;

namespace BalanceProjection.Domain.Repositories;

public interface IBalanceReportRepository
{
    Task<AccountBalanceReport?> GetAsync(int accountId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccountBalanceReport>> ListAsync(int skip, int take, CancellationToken cancellationToken);
}
