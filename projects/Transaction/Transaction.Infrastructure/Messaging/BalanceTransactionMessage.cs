using System.Text.Json;
using RabbitMQ.Client;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.Messaging.Events;

namespace Transaction.Infrastructure.Messaging;

/// <summary>Builds the RabbitMQ message for an outbox entry.</summary>
internal static class BalanceTransactionMessage
{
    public static (BasicProperties Properties, byte[] Body) Create(BalanceTransaction entry) =>
        (new BasicProperties
        {
            // At-least-once delivery: the same entry may be sent more than once.
            // A stable MessageId lets consumers drop the repeat.
            MessageId = entry.Id.ToString(),
            ContentType = "application/json",
            Type = nameof(BalanceTransactionCreatedEvent),
            DeliveryMode = DeliveryModes.Persistent,
        },
        JsonSerializer.SerializeToUtf8Bytes(BalanceTransactionCreatedEvent.From(entry)));
}
