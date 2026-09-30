using BalanceProjection.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BalanceProjection.IoC.Registrations;

internal static class MessagingRegistration
{
    internal static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        // The connection is thread-safe and meant to live for the whole application.
        services.AddSingleton<RabbitMqConnectionProvider>();

        services.AddHostedService<BalanceEventProcessor>();
        services.AddHostedService<BusinessAckPublisher>();
        return services;
    }
}
