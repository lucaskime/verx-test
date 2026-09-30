using System.ComponentModel.DataAnnotations;

namespace Transaction.IoC.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Npgsql transient-failure retries before giving up on a connection/command.</summary>
    public int MaxRetryCount { get; init; } = 6;

    /// <summary>Upper bound on the exponential backoff delay between retries.</summary>
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);
}
