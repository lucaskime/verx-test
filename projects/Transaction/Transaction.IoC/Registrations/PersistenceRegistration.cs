using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Persistence.Contexts;
using Transaction.Infrastructure.Persistence.Repositories;
using Transaction.IoC.Options;

namespace Transaction.IoC.Registrations;

internal static class PersistenceRegistration
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        return services
            .AddDbContext<TransactionDbContext>((serviceProvider, options) =>
            {
                var database = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseNpgsql(database.ConnectionString, npgsql =>
                    npgsql.EnableRetryOnFailure(database.MaxRetryCount, database.MaxRetryDelay, errorCodesToAdd: null));
            })
            .AddScoped<ITransactionRepository, TransactionRepository>();
    }
}
