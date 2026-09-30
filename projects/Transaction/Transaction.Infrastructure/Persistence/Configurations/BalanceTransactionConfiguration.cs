using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.Persistence.Contexts;

namespace Transaction.Infrastructure.Persistence.Configurations;

internal sealed class BalanceTransactionConfiguration : IEntityTypeConfiguration<BalanceTransaction>
{
    public void Configure(EntityTypeBuilder<BalanceTransaction> builder)
    {
        builder.ToTable(nameof(TransactionDbContext.BalanceTransactions), Schemas.Balance);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<RawTransaction>()
            .WithOne()
            .HasForeignKey<BalanceTransaction>(x => x.RawTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Idempotency: one balance entry per raw transaction.
        builder.HasIndex(x => x.RawTransactionId).IsUnique();

        // Outbox queue: entries not yet published.
        builder.HasIndex(x => x.CreatedAt)
            .HasFilter($"\"{nameof(BalanceTransaction.PublishedAt)}\" IS NULL");

        builder.HasIndex(x => new { x.AccountId, x.CreatedAt });
    }
}
