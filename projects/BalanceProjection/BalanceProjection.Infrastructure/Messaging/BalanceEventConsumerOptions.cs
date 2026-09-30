using System.ComponentModel.DataAnnotations;

namespace BalanceProjection.Infrastructure.Messaging;

public sealed class BalanceEventConsumerOptions
{
    public const string SectionName = "BalanceEventConsumer";

    [Range(1, 64)]
    public int MaxConcurrentCalls { get; init; } = 8;
}
