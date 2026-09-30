using Microsoft.Extensions.DependencyInjection;
using Transaction.IoC.HealthChecks;

namespace Transaction.IoC.Registrations;

internal static class HealthCheckRegistration
{
    internal static IServiceCollection AddDependencyHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"], timeout: TimeSpan.FromSeconds(5))
            .AddCheck<RabbitMqHealthCheck>("eventbus", tags: ["ready"], timeout: TimeSpan.FromSeconds(5));

        return services;
    }
}
