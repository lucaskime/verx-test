using Microsoft.Extensions.DependencyInjection;
using Transaction.Infrastructure.Messaging;
using Transaction.IoC.Options;

namespace Transaction.IoC.Registrations;

internal static class SettingsRegistration
{
    internal static IServiceCollection AddSettings(this IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<OutboxPublisherOptions>()
            .BindConfiguration(OutboxPublisherOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<BalanceProcessedConsumerOptions>()
            .BindConfiguration(BalanceProcessedConsumerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
