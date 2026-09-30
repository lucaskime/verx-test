using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Transaction.WebApi.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("health")]
    public Task<IActionResult> Get(
        [FromServices] HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        WriteReportAsync(healthCheckService, _ => true, cancellationToken);

    [HttpGet("health/ready")]
    public Task<IActionResult> Ready(
        [FromServices] HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        WriteReportAsync(healthCheckService, registration => registration.Tags.Contains("ready"), cancellationToken);

    [HttpGet("health/live")]
    public Task<IActionResult> Live(
        [FromServices] HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
        WriteReportAsync(healthCheckService, _ => false, cancellationToken);

    private static async Task<IActionResult> WriteReportAsync(
        HealthCheckService healthCheckService,
        Func<HealthCheckRegistration, bool> predicate,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(predicate, cancellationToken);
        var statusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return new ObjectResult(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(entry => entry.Key, entry => new
            {
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
            }),
        })
        {
            StatusCode = statusCode,
        };
    }
}
