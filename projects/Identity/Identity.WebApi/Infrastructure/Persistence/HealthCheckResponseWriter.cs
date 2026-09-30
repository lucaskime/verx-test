using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.WebApi.Infrastructure.Persistence;

/// <summary>
/// Resposta detalhada do /health: status geral e, por check, descrição, erro e dados.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext httpContext, HealthReport report) =>
        httpContext.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                error = entry.Value.Exception?.GetBaseException().Message,
                durationMs = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data,
            }),
        });
}
