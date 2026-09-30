using System.ComponentModel.DataAnnotations;

namespace Transaction.Infrastructure.Messaging;

public sealed class BalanceProcessedConsumerOptions
{
    public const string SectionName = "BalanceProcessedConsumer";

    [Range(1, 64)]
    public int MaxConcurrentCalls { get; init; } = 8;
}
