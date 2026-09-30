using System.Text.Json;
using BalanceProjection.Domain.Entities;
using BalanceProjection.Domain.Transactions;
using BalanceProjection.Infrastructure.Messaging.Events;
using BalanceProjection.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BalanceProjection.Infrastructure.Messaging;

/// <summary>
/// Applies Transaction events to the reporting projection. Database commit is the business ACK;
/// the broker message is completed only after that commit succeeds.
/// </summary>
public sealed class BalanceEventProcessor : IHostedService, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqConnectionProvider _rabbitMq;
    private readonly BalanceEventConsumerOptions _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BalanceEventProcessor> _logger;
    private IChannel? _channel;

    public BalanceEventProcessor(
        RabbitMqConnectionProvider rabbitMq,
        IOptions<BalanceEventConsumerOptions> options,
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<BalanceEventProcessor> logger)
    {
        _rabbitMq = rabbitMq;
        _settings = options.Value;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await _rabbitMq.GetConnectionAsync(cancellationToken);
        var concurrency = (ushort)_settings.MaxConcurrentCalls;
        _channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: false, publisherConfirmationTrackingEnabled: false,
                consumerDispatchConcurrency: concurrency),
            cancellationToken);
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: concurrency, global: false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += ProcessMessageAsync;
        await _channel.BasicConsumeAsync(
            RabbitMqTopology.BalanceProjectionQueue, autoAck: false, consumer, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        var channel = _channel!;
        var messageId = args.BasicProperties.MessageId;

        BalanceTransactionCreatedEvent? message;
        try
        {
            message = JsonSerializer.Deserialize<BalanceTransactionCreatedEvent>(args.Body.Span, SerializerOptions);
            Validate(message, messageId);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException)
        {
            // requeue: false sends the message to the dead-letter queue.
            _logger.LogWarning(ex, "Dead-lettering invalid balance event {MessageId}.", messageId);
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, args.CancellationToken);
            return;
        }

        try
        {
            await ApplyAsync(message!, args.CancellationToken);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, args.CancellationToken);
            _logger.LogInformation(
                "Processed balance transaction {TransactionId} for account {AccountId}.", message!.Id, message.AccountId);
        }
        catch (Exception ex) when (!args.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Failed to process balance transaction {TransactionId}; it will be retried.", message!.Id);
            // Redelivery is counted by the quorum queue; after the delivery limit it is dead-lettered.
            await Task.Delay(TimeSpan.FromSeconds(1), args.CancellationToken);
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, args.CancellationToken);
        }
    }

    private async Task ApplyAsync(BalanceTransactionCreatedEvent message, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BalanceProjectionDbContext>();

        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            // Transaction-owned application locks serialize duplicate deliveries and account updates.
            await AcquireLockAsync(db, $"balance-event:{message.Id:D}", cancellationToken);

            var alreadyProcessed = await db.ProcessedTransactions
                .AnyAsync(x => x.TransactionId == message.Id, cancellationToken);

            if (!alreadyProcessed)
            {
                var processedAt = _timeProvider.GetUtcNow();
                var credit = message.Type == TransactionType.Credit ? message.Amount : 0m;
                var debit = message.Type == TransactionType.Debit ? message.Amount : 0m;
                var delta = credit - debit;

                await AcquireLockAsync(db, $"balance-account:{message.AccountId}", cancellationToken);
                await db.Database.ExecuteSqlInterpolatedAsync($$"""
                    INSERT INTO balance_projection."AccountBalances"
                        ("AccountId", "Balance", "TotalCredits", "TotalDebits", "TransactionCount",
                         "LastTransactionAt", "UpdatedAt")
                    VALUES
                        ({{message.AccountId}}, {{delta}}, {{credit}}, {{debit}}, 1,
                         {{message.CreatedAt.ToUniversalTime()}}, {{processedAt.ToUniversalTime()}})
                    ON CONFLICT ("AccountId") DO UPDATE SET
                        "Balance" = balance_projection."AccountBalances"."Balance" + EXCLUDED."Balance",
                        "TotalCredits" = balance_projection."AccountBalances"."TotalCredits" + EXCLUDED."TotalCredits",
                        "TotalDebits" = balance_projection."AccountBalances"."TotalDebits" + EXCLUDED."TotalDebits",
                        "TransactionCount" = balance_projection."AccountBalances"."TransactionCount" + 1,
                        "LastTransactionAt" = GREATEST(
                            balance_projection."AccountBalances"."LastTransactionAt", EXCLUDED."LastTransactionAt"),
                        "UpdatedAt" = EXCLUDED."UpdatedAt"
                    """, cancellationToken);

                db.ProcessedTransactions.Add(new ProcessedTransaction
                {
                    TransactionId = message.Id,
                    RawTransactionId = message.RawTransactionId,
                    AccountId = message.AccountId,
                    ProcessedAt = processedAt,
                });
                // Written atomically with the projection. A separate outbox publisher sends it to Transaction.
                db.BusinessAcks.Add(new BusinessAck
                {
                    TransactionId = message.Id,
                    RawTransactionId = message.RawTransactionId,
                    AccountId = message.AccountId,
                    ProcessedAt = processedAt,
                });

                await db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task AcquireLockAsync(
        BalanceProjectionDbContext db, string resource, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({resource}, 0))", cancellationToken);
    }

    private static void Validate(BalanceTransactionCreatedEvent? message, string? messageId)
    {
        if (message is null)
            throw new JsonException("The message body is empty.");
        if (message.Id == Guid.Empty || message.RawTransactionId == Guid.Empty || message.AccountId <= 0)
            throw new ArgumentException("The event contains invalid identifiers.");
        if (message.Amount <= 0 || !Enum.IsDefined(message.Type))
            throw new ArgumentException("The event contains an invalid amount or transaction type.");
        if (!string.Equals(messageId, message.Id.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("MessageId must match the event Id.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}
