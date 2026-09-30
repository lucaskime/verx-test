using BalanceProjection.Domain.Repositories;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using BalanceProjection.Infrastructure.Persistence.Repositories;
using BalanceProjection.IoC.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BalanceProjection.IoC.Registrations;

internal static class PersistenceRegistration
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        return services
            .AddDbContext<BalanceProjectionDbContext>((serviceProvider, options) =>
            {
                var database = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseNpgsql(database.ConnectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(database.MaxRetryCount, database.MaxRetryDelay, errorCodesToAdd: null);
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "balance_projection");
                });
            })
            .AddScoped<IBalanceReportRepository, BalanceReportRepository>();
    }
}
