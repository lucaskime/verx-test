using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.Persistence;
using Transaction.Infrastructure.Persistence.Contexts;

namespace Transaction.Infrastructure.Messaging;

/// <summary>
/// Outbox relay and the only publishing path: sends the <see cref="BalanceTransaction"/> entries not yet
/// published and fills <see cref="BalanceTransaction.PublishedAt"/> with the broker publisher confirm.
/// </summary>
public sealed class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    RabbitMqConnectionProvider rabbitMq,
    IOptions<OutboxPublisherOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    // SKIP LOCKED: concurrent instances each take a different batch, so no row is published twice at the same time.
    // FromSql materializes a tracked entity, so every mapped column must be selected.
    private const string SelectPendingSql = $$"""
        SELECT
            "{{nameof(BalanceTransaction.Id)}}",
            "{{nameof(BalanceTransaction.RawTransactionId)}}",
            "{{nameof(BalanceTransaction.Type)}}",
            "{{nameof(BalanceTransaction.AccountId)}}",
            "{{nameof(BalanceTransaction.Amount)}}",
            "{{nameof(BalanceTransaction.PublishedAt)}}",
            "{{nameof(BalanceTransaction.ProcessedAt)}}",
            "{{nameof(BalanceTransaction.CreatedBy)}}",
            "{{nameof(BalanceTransaction.CreatedAt)}}"
        FROM {{Schemas.Balance}}."{{nameof(TransactionDbContext.BalanceTransactions)}}"
        WHERE "{{nameof(BalanceTransaction.PublishedAt)}}" IS NULL
        ORDER BY "{{nameof(BalanceTransaction.CreatedAt)}}"
        LIMIT {0}
        FOR UPDATE SKIP LOCKED
        """;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        using var timer = new PeriodicTimer(settings.PollingInterval, timeProvider);

        do
        {
            try
            {
                // Drains the backlog before waiting for the next tick.
                while (await PublishPendingAsync(settings.BatchSize, stoppingToken) == settings.BatchSize)
                {
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Failed to publish outbox entries; retrying in {PollingInterval}.", settings.PollingInterval);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<int> PublishPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        // EnableRetryOnFailure requires explicit transactions to run inside the execution strategy.
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var pending = await db.BalanceTransactions
                .FromSqlRaw(SelectPendingSql, batchSize)
                .ToListAsync(cancellationToken);

            if (pending.Count == 0)
                return 0;

            // Opened inside the retry loop so a broker outage cannot stop the host.
            // Publisher confirms: BasicPublishAsync throws unless the broker has accepted the message.
            await using var channel = await rabbitMq.CreatePublisherChannelAsync(cancellationToken);
            foreach (var entry in pending)
            {
                var (properties, body) = BalanceTransactionMessage.Create(entry);
                await channel.BasicPublishAsync(
                    RabbitMqTopology.BalanceTransactionsExchange, routingKey: string.Empty,
                    mandatory: false, properties, body, cancellationToken);
            }

            var publishedAt = timeProvider.GetUtcNow();
            foreach (var entry in pending)
                entry.PublishedAt = publishedAt;

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Published {Count} outbox entries.", pending.Count);
            return pending.Count;
        });
    }
}
