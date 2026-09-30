using BalanceProjection.Domain.Entities;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalanceProjection.Infrastructure.Persistence.Configurations;

internal sealed class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable(nameof(BalanceProjectionDbContext.AccountBalances), Schemas.Projection);
        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.AccountId).ValueGeneratedNever();
        builder.Property(x => x.Balance).HasPrecision(18, 2);
        builder.Property(x => x.TotalCredits).HasPrecision(18, 2);
        builder.Property(x => x.TotalDebits).HasPrecision(18, 2);
    }
}
