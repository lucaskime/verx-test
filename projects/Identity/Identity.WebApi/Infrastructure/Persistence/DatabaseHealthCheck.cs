using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.WebApi.Infrastructure.Persistence;

public sealed class DatabaseHealthCheck(IdentityDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
            return HealthCheckResult.Healthy($"Banco acessível; {users} usuários cadastrados.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Não foi possível consultar o banco do Identity.", exception);
        }
    }
}
