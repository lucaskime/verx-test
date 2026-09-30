using BalanceProjection.Infrastructure.Messaging;
using BalanceProjection.IoC.Options;
using Microsoft.Extensions.DependencyInjection;

namespace BalanceProjection.IoC.Registrations;

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
            .AddOptions<BalanceEventConsumerOptions>()
            .BindConfiguration(BalanceEventConsumerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<BusinessAckPublisherOptions>()
            .BindConfiguration(BusinessAckPublisherOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
