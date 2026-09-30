using Microsoft.Extensions.DependencyInjection;

namespace Transaction.IoC.Registrations;

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
