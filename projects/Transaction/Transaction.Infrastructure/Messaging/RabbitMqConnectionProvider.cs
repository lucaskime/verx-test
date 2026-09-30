using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Transaction.Infrastructure.Messaging;

/// <summary>
/// Lazily opens the single, application-wide RabbitMQ connection (thread-safe, with automatic recovery)
/// and declares the topology, so a broker that is down at startup does not stop the host.
/// </summary>
public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            return _connection;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not null)
                return _connection;

            var settings = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = settings.Host,
                Port = settings.Port,
                UserName = settings.User,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
            };

            var connection = await factory.CreateConnectionAsync(cancellationToken);
            try
            {
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await RabbitMqTopology.EnsureAsync(channel, cancellationToken);
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }

            _connection = connection;
            return connection;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Channel with publisher confirms: a publish completes only after the broker has persisted it.</summary>
    public async Task<IChannel> CreatePublisherChannelAsync(CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        return await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        _gate.Dispose();
    }
}
