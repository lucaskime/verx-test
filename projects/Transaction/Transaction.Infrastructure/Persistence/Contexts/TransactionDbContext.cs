using Microsoft.EntityFrameworkCore;
using Transaction.Domain.Entities;

namespace Transaction.Infrastructure.Persistence.Contexts;

/// <summary>The service's database context. Owner of the migrations.</summary>
public sealed class TransactionDbContext(DbContextOptions<TransactionDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RawTransaction> RawTransactions => Set<RawTransaction>();
    public DbSet<BalanceTransaction> BalanceTransactions => Set<BalanceTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }
}
