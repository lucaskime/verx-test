using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Transaction.Infrastructure.Messaging;

namespace Transaction.IoC.Registrations;

internal static class MessagingRegistration
{
    internal static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        // The connection is thread-safe and meant to live for the whole application.
        services.AddSingleton<RabbitMqConnectionProvider>();

        services.AddHostedService<OutboxPublisher>();
        services.AddHostedService<BalanceProcessedConsumer>();
        return services;
    }
}
