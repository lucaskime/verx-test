using BalanceProjection.Domain.Reports;
using BalanceProjection.Domain.Repositories;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BalanceProjection.Infrastructure.Persistence.Repositories;

public sealed class BalanceReportRepository(BalanceProjectionDbContext db) : IBalanceReportRepository
{
    public Task<AccountBalanceReport?> GetAsync(int accountId, CancellationToken cancellationToken) =>
        db.AccountBalances.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => new AccountBalanceReport(x.AccountId, x.Balance, x.TotalCredits,
                x.TotalDebits, x.TransactionCount, x.LastTransactionAt, x.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AccountBalanceReport>> ListAsync(
        int skip, int take, CancellationToken cancellationToken) =>
        await db.AccountBalances.AsNoTracking()
            .OrderBy(x => x.AccountId)
            .Skip(skip)
            .Take(take)
            .Select(x => new AccountBalanceReport(x.AccountId, x.Balance, x.TotalCredits,
                x.TotalDebits, x.TransactionCount, x.LastTransactionAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
}
