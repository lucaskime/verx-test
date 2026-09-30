using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.Messaging.Events;
using Transaction.Infrastructure.Persistence.Contexts;

namespace Transaction.Infrastructure.Messaging;

/// <summary>
/// Consumes the BalanceProjection business ACK and records it in Transaction's own outbox row.
/// Message settlement happens only after the database write succeeds.
/// </summary>
public sealed class BalanceProcessedConsumer : IHostedService, IAsyncDisposable
{
    private readonly RabbitMqConnectionProvider _rabbitMq;
    private readonly BalanceProcessedConsumerOptions _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BalanceProcessedConsumer> _logger;
    private IChannel? _channel;

    public BalanceProcessedConsumer(
        RabbitMqConnectionProvider rabbitMq,
        IOptions<BalanceProcessedConsumerOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BalanceProcessedConsumer> logger)
    {
        _rabbitMq = rabbitMq;
        _settings = options.Value;
        _scopeFactory = scopeFactory;
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
            RabbitMqTopology.TransactionBalanceProcessedQueue, autoAck: false, consumer, cancellationToken);
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

        BalanceTransactionProcessedEvent? message;
        try
        {
            message = JsonSerializer.Deserialize<BalanceTransactionProcessedEvent>(args.Body.Span);
            Validate(message, messageId);
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or InvalidOperationException)
        {
            // requeue: false sends the message to the dead-letter queue.
            _logger.LogWarning(ex, "Dead-lettering invalid balance business ACK {MessageId}.", messageId);
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, args.CancellationToken);
            return;
        }

        try
        {
            await RecordProcessedAtAsync(message!, args.CancellationToken);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, args.CancellationToken);
            _logger.LogInformation(
                "Recorded business ACK for balance transaction {TransactionId} at {ProcessedAt}.",
                message!.TransactionId, message.ProcessedAt);
        }
        catch (Exception ex) when (!args.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex,
                "Failed to record business ACK for balance transaction {TransactionId}; it will be retried.",
                message!.TransactionId);
            // Redelivery is counted by the quorum queue; after the delivery limit it is dead-lettered.
            await Task.Delay(TimeSpan.FromSeconds(1), args.CancellationToken);
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, args.CancellationToken);
        }
    }

    private async Task RecordProcessedAtAsync(
        BalanceTransactionProcessedEvent message,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var updated = await db.BalanceTransactions
            .Where(x => x.Id == message.TransactionId
                && x.RawTransactionId == message.RawTransactionId
                && x.AccountId == message.AccountId
                && x.ProcessedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ProcessedAt, message.ProcessedAt), cancellationToken);

        if (updated == 1)
            return;

        // A repeated ACK is success only when it identifies the exact same source entry.
        var existing = await db.BalanceTransactions.AsNoTracking()
            .Where(x => x.Id == message.TransactionId)
            .Select(x => new { x.RawTransactionId, x.AccountId, x.ProcessedAt })
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is null)
            throw new InvalidOperationException($"Balance transaction '{message.TransactionId}' was not found.");

        if (existing.RawTransactionId != message.RawTransactionId || existing.AccountId != message.AccountId)
            throw new InvalidOperationException(
                $"Business ACK '{message.TransactionId}' does not match its source transaction.");

        if (existing.ProcessedAt is null)
            throw new InvalidOperationException(
                $"Balance transaction '{message.TransactionId}' could not be acknowledged.");
    }

    private static void Validate(BalanceTransactionProcessedEvent? message, string? messageId)
    {
        if (message is null)
            throw new JsonException("The message body is empty.");
        if (message.TransactionId == Guid.Empty || message.RawTransactionId == Guid.Empty || message.AccountId <= 0)
            throw new ArgumentException("The business ACK contains invalid identifiers.");
        if (message.ProcessedAt == default)
            throw new ArgumentException("The business ACK contains an invalid processing timestamp.");
        if (!string.Equals(messageId, message.TransactionId.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("MessageId must match TransactionId.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}
