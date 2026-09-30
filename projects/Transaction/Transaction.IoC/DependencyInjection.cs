using Microsoft.Extensions.DependencyInjection;
using Transaction.IoC.Registrations;

namespace Transaction.IoC;

public static class DependencyInjection
{
    // Single composition point: the WebApi calls only this method.
    public static IServiceCollection AddTransaction(this IServiceCollection services)
    {
        return services
            .AddSettings()
            .AddApplication()
            .AddInfrastructure();
    }
}
