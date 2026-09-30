using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Transaction.Application.Transactions;

namespace Transaction.IoC.Registrations;

internal static class ApplicationRegistration
{
    internal static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        return services.AddScoped<CreateTransactionHandler>();
    }
}
