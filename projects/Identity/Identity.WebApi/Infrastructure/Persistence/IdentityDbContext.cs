using Identity.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.WebApi.Infrastructure.Persistence;

/// <summary>Contexto do banco do Identity. Dono das migrations (schema "identity").</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
