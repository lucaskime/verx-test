using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.Persistence.Contexts;

namespace Transaction.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable(nameof(TransactionDbContext.Accounts), Schemas.Transaction);

        builder.HasKey(x => x.Id);
    }
}
