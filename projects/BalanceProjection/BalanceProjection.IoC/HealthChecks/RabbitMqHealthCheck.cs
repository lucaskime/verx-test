using BalanceProjection.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BalanceProjection.IoC.HealthChecks;

internal sealed class RabbitMqHealthCheck(RabbitMqConnectionProvider provider) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await provider.GetConnectionAsync(cancellationToken);
            return connection.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ connection is open.")
                : HealthCheckResult.Unhealthy("RabbitMQ connection is closed (recovering).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                cancellationToken.IsCancellationRequested
                    ? "RabbitMQ health check timed out or was canceled."
                    : "RabbitMQ connection failed.", ex);
        }
    }
}
