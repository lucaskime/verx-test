using BalanceProjection.Application.Reports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BalanceProjection.IoC.Registrations;

internal static class ApplicationRegistration
{
    internal static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        return services
            .AddScoped<GetAccountBalanceHandler>()
            .AddScoped<ListAccountBalancesHandler>();
    }
}
