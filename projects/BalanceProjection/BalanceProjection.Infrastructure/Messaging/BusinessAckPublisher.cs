using System.Text.Json;
using RabbitMQ.Client;
using BalanceProjection.Domain.Entities;
using BalanceProjection.Infrastructure.Messaging.Events;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BalanceProjection.Infrastructure.Messaging;

/// <summary>
/// Outbox relay and the only publishing path: sends the <see cref="BusinessAck"/> entries not yet
/// published and fills <see cref="BusinessAck.PublishedAt"/> with the broker publisher confirm.
/// </summary>
public sealed class BusinessAckPublisher(
    IServiceScopeFactory scopeFactory,
    RabbitMqConnectionProvider rabbitMq,
    IOptions<BusinessAckPublisherOptions> options,
    TimeProvider timeProvider,
    ILogger<BusinessAckPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        using var timer = new PeriodicTimer(settings.PollingInterval, timeProvider);

        do
        {
            try
            {
                // Drains the backlog before waiting for the next tick.
                while (await PublishBatchAsync(settings.BatchSize, stoppingToken) == settings.BatchSize)
                {
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Failed to publish business ACKs; retrying in {Interval}.", settings.PollingInterval);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<int> PublishBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BalanceProjectionDbContext>();

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var pending = await db.BusinessAcks
                .Where(ack => ack.PublishedAt == null)
                .OrderBy(ack => ack.ProcessedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (pending.Count == 0)
                return 0;

            // Publisher confirms: BasicPublishAsync throws unless the broker has accepted the message.
            await using var channel = await rabbitMq.CreatePublisherChannelAsync(cancellationToken);
            foreach (var ack in pending)
            {
                var (properties, body) = CreateMessage(ack);
                await channel.BasicPublishAsync(
                    RabbitMqTopology.BalanceTransactionProcessedExchange, routingKey: string.Empty,
                    mandatory: false, properties, body, cancellationToken);
            }

            var publishedAt = timeProvider.GetUtcNow();
            foreach (var ack in pending)
                ack.PublishedAt = publishedAt;

            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Published {Count} balance business ACKs.", pending.Count);
            return pending.Count;
        });
    }

    private static (BasicProperties Properties, byte[] Body) CreateMessage(BusinessAck ack) =>
        (new BasicProperties
        {
            // At-least-once delivery: the same entry may be sent more than once.
            // A stable MessageId lets consumers drop the repeat.
            MessageId = ack.TransactionId.ToString(),
            Type = nameof(BalanceTransactionProcessedEvent),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
        },
        JsonSerializer.SerializeToUtf8Bytes(new BalanceTransactionProcessedEvent(
            ack.TransactionId, ack.RawTransactionId, ack.AccountId, ack.ProcessedAt)));
}
