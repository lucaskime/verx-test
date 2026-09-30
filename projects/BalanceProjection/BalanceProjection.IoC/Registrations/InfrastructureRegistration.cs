using Microsoft.Extensions.DependencyInjection;

namespace BalanceProjection.IoC.Registrations;

internal static class InfrastructureRegistration
{
    internal static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services
            .AddPersistence()
            .AddMessaging()
            .AddDependencyHealthChecks();
    }
}
