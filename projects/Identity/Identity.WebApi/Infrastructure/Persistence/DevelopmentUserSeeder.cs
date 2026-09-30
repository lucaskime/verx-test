using System.Text.Json;
using Identity.WebApi.Domain.Entities;
using Identity.WebApi.Infrastructure.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Identity.WebApi.Infrastructure.Persistence;

/// <summary>
/// Somente em Development: se a tabela de usuários estiver vazia, cria 50 usuários de teste
/// e grava as credenciais em texto claro no arquivo configurado (ignorado pelo git).
/// </summary>
public sealed class DevelopmentUserSeeder(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IHostEnvironment environment) : IHostedService
{
    private const string TestPasswordPrefix = "TestUser";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return;

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        if (await dbContext.Users.AnyAsync(cancellationToken))
            return;

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var credentials = Enumerable.Range(1, 50)
            .Select(number => new TestCredential(
                $"user{number:D3}@example.local",
                $"{TestPasswordPrefix}{number:D3}!"))
            .ToArray();

        var credentialsPath = Path.GetFullPath(
            configuration["DevelopmentSeed:TestCredentialsFilePath"] ?? "Data/test-credentials.json",
            environment.ContentRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(credentialsPath)!);
        await using (var stream = File.Create(credentialsPath))
            await JsonSerializer.SerializeAsync(stream, new TestCredentialsFile(credentials), JsonOptions, cancellationToken);

        dbContext.Users.AddRange(credentials.Select(credential =>
            new User(credential.Email, passwordHasher.Hash(credential.Password))));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed record TestCredentialsFile(TestCredential[] Users);
    private sealed record TestCredential(string Email, string Password);
}
