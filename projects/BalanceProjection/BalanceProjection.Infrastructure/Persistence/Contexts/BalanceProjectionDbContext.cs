using BalanceProjection.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalanceProjection.Infrastructure.Persistence.Contexts;

/// <summary>The service's database context. Owner of the migrations.</summary>
public sealed class BalanceProjectionDbContext(DbContextOptions<BalanceProjectionDbContext> options)
    : DbContext(options)
{
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();
    public DbSet<ProcessedTransaction> ProcessedTransactions => Set<ProcessedTransaction>();
    public DbSet<BusinessAck> BusinessAcks => Set<BusinessAck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BalanceProjectionDbContext).Assembly);
}
