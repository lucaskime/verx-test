using System.ComponentModel.DataAnnotations;

namespace BalanceProjection.Infrastructure.Messaging;

public sealed class BusinessAckPublisherOptions
{
    public const string SectionName = "BusinessAck";

    /// <summary>Wait between polls when there is nothing left to publish.</summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "01:00:00")]
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Maximum entries sent per publish batch.</summary>
    [Range(1, 1000)]
    public int BatchSize { get; init; } = 100;
}
