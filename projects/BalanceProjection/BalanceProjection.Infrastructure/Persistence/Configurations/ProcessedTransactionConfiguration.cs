using BalanceProjection.Domain.Entities;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalanceProjection.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedTransactionConfiguration : IEntityTypeConfiguration<ProcessedTransaction>
{
    public void Configure(EntityTypeBuilder<ProcessedTransaction> builder)
    {
        builder.ToTable(nameof(BalanceProjectionDbContext.ProcessedTransactions), Schemas.Projection);
        builder.HasKey(x => x.TransactionId);
        builder.Property(x => x.TransactionId).ValueGeneratedNever();
        builder.HasIndex(x => x.RawTransactionId).IsUnique();
        builder.HasIndex(x => x.ProcessedAt);
    }
}
