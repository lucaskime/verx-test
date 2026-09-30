using Microsoft.EntityFrameworkCore;
using Npgsql;
using Transaction.Domain.Entities;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Persistence.Configurations;
using Transaction.Infrastructure.Persistence.Contexts;

namespace Transaction.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(TransactionDbContext db) : ITransactionRepository
{
    public async Task AddAsync(RawTransaction transaction, CancellationToken cancellationToken)
    {
        db.RawTransactions.Add(transaction);

        // Outbox entry: pending until the OutboxPublisher gets the broker publisher confirm.
        db.BalanceTransactions.Add(BalanceTransaction.From(transaction));

        try
        {
            // A single SaveChanges runs in one database transaction: both rows are written, or neither.
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: RawTransactionConfiguration.IdempotencyIndexName,
        })
        {
            // Drop the rejected rows so the context can still be queried (the caller reads the winner).
            db.ChangeTracker.Clear();
            throw new DuplicateIdempotencyKeyException(ex);
        }
    }

    public Task<RawTransaction?> FindByIdempotencyKeyAsync(string scope, string key, CancellationToken cancellationToken) =>
        db.RawTransactions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyScope == scope && x.IdempotencyKey == key, cancellationToken);
}
