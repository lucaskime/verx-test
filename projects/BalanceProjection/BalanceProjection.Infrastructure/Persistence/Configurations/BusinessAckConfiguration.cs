using BalanceProjection.Domain.Entities;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalanceProjection.Infrastructure.Persistence.Configurations;

internal sealed class BusinessAckConfiguration : IEntityTypeConfiguration<BusinessAck>
{
    public void Configure(EntityTypeBuilder<BusinessAck> builder)
    {
        builder.ToTable(nameof(BalanceProjectionDbContext.BusinessAcks), Schemas.Projection);
        builder.HasKey(x => x.TransactionId);
        builder.Property(x => x.TransactionId).ValueGeneratedNever();
        builder.HasIndex(x => x.ProcessedAt)
            .HasFilter($"\"{nameof(BusinessAck.PublishedAt)}\" IS NULL");
    }
}
