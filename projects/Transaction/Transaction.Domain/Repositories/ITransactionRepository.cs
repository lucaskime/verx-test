using Transaction.Domain.Entities;

namespace Transaction.Domain.Repositories;

public interface ITransactionRepository
{
    /// <summary>
    /// Persists the transaction together with its balance entry (outbox) in a single database transaction.
    /// </summary>
    /// <exception cref="DuplicateIdempotencyKeyException">Another transaction already holds the same scope and key.</exception>
    Task AddAsync(RawTransaction transaction, CancellationToken cancellationToken);

    /// <summary>The transaction recorded under this idempotency scope and key, or null.</summary>
    Task<RawTransaction?> FindByIdempotencyKeyAsync(string scope, string key, CancellationToken cancellationToken);
}
