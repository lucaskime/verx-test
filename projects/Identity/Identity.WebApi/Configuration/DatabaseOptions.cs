using System.ComponentModel.DataAnnotations;

namespace Identity.WebApi.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Tentativas do Npgsql em falhas transitórias antes de desistir.</summary>
    public int MaxRetryCount { get; init; } = 6;

    /// <summary>Limite do backoff exponencial entre as tentativas.</summary>
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);
}
