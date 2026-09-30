using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.Persistence.Contexts;

namespace Transaction.Infrastructure.Persistence.Configurations;

internal sealed class RawTransactionConfiguration : IEntityTypeConfiguration<RawTransaction>
{
    internal const string IdempotencyIndexName = "IX_RawTransactions_IdempotencyScope_IdempotencyKey";

    public void Configure(EntityTypeBuilder<RawTransaction> builder)
    {
        builder.ToTable(nameof(TransactionDbContext.RawTransactions), Schemas.Transaction);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.AccountId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.IdempotencyScope).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(255).IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => new { x.AccountId, x.CreatedAt });

        // Idempotency: a key is used once per caller. Concurrent duplicates are settled here, by the database.
        builder.HasIndex(x => new { x.IdempotencyScope, x.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName(IdempotencyIndexName);
    }
}
