using BalanceProjection.IoC.Registrations;
using Microsoft.Extensions.DependencyInjection;

namespace BalanceProjection.IoC;

public static class DependencyInjection
{
    // Single composition point: the WebApi calls only this method.
    public static IServiceCollection AddBalanceProjection(this IServiceCollection services)
    {
        return services
            .AddSettings()
            .AddApplication()
            .AddInfrastructure();
    }
}
