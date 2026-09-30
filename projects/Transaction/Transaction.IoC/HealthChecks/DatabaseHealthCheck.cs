using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using Transaction.IoC.Options;

namespace Transaction.IoC.HealthChecks;

internal sealed class DatabaseHealthCheck(IOptions<DatabaseOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // A direct query avoids the application's long EF retry policy.
            await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL connection and query succeeded.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                cancellationToken.IsCancellationRequested
                    ? "PostgreSQL health check timed out or was canceled."
                    : "PostgreSQL connection or query failed.", ex);
        }
    }
}
