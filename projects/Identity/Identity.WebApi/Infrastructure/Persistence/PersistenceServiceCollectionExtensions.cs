using Identity.WebApi.Configuration;
using Identity.WebApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.WebApi.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            var database = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(database.ConnectionString, npgsql =>
                npgsql
                    .MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Identity)
                    .EnableRetryOnFailure(database.MaxRetryCount, database.MaxRetryDelay, errorCodesToAdd: null));
        });

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddHostedService<DevelopmentUserSeeder>();
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
        services.AddProblemDetails();

        return services;
    }
}
