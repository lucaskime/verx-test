using RabbitMQ.Client;

namespace BalanceProjection.Infrastructure.Messaging;

/// <summary>
/// Topology shared by Transaction and BalanceProjection. Both services declare all of it (idempotently, with
/// identical arguments) so events are never dropped because the peer has not started yet.
/// </summary>
public static class RabbitMqTopology
{
    public const string BalanceTransactionsExchange = "balance-transactions";
    public const string BalanceTransactionProcessedExchange = "balance-transaction-processed";
    public const string DeadLetterExchange = "verx.dead-letter";

    public const string BalanceProjectionQueue = "balance-projection";
    public const string TransactionBalanceProcessedQueue = "transaction-balance-processed";

    /// <summary>Deliveries before a repeatedly failing message is dead-lettered.</summary>
    private const int DeliveryLimit = 10;

    public static async Task EnsureAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(BalanceTransactionsExchange, ExchangeType.Fanout, durable: true,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(BalanceTransactionProcessedExchange, ExchangeType.Fanout, durable: true,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Direct, durable: true,
            cancellationToken: cancellationToken);

        await DeclareQueueAsync(channel, BalanceProjectionQueue, BalanceTransactionsExchange, cancellationToken);
        await DeclareQueueAsync(channel, TransactionBalanceProcessedQueue, BalanceTransactionProcessedExchange,
            cancellationToken);
    }

    private static async Task DeclareQueueAsync(
        IChannel channel, string queue, string exchange, CancellationToken cancellationToken)
    {
        var deadLetterQueue = $"{queue}.dead-letter";

        await channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-queue-type"] = "quorum" },
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(deadLetterQueue, DeadLetterExchange, queue, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-delivery-limit"] = DeliveryLimit,
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-routing-key"] = queue,
            },
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue, exchange, routingKey: string.Empty, cancellationToken: cancellationToken);
    }
}
