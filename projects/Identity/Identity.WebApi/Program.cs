using Identity.WebApi.Configuration;
using Identity.WebApi.Features;
using Identity.WebApi.Infrastructure.Persistence;
using Identity.WebApi.Infrastructure.Security;
using Identity.WebApi.Infrastructure.Security.OpenApi;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace Identity.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerOpenApiTransformer>();
            options.AddOperationTransformer<BearerOpenApiTransformer>();
        });

        builder.Services
            .AddAppOptions()
            .AddPersistence()
            .AddSecurity()
            .AddFeatures();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // Scalar servido na raiz ("/") para abrir direto ao iniciar o debug.
            app.MapScalarApiReference("/");
        }

        app.UseHttpsRedirection();

        // Autenticação precede autorização, e ambas vêm antes do mapeamento dos endpoints.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
        });

        await app.RunAsync();
    }
}
