using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Transaction.Application.Results;
using Transaction.Application.Validation;

namespace Transaction.Application.Transactions;

// Rules for the Idempotency-Key header and the fingerprint that tells a retry from a key reused for other data.
internal static class IdempotencyKeys
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReusedCode = "Idempotency.KeyReused";
    public const int MaxLength = 255;

    public static Result Validate(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return ValidationErrors.Invalid($"The {HeaderName} header is required.");

        // Visible ASCII only: no spaces, control or non-ASCII characters.
        if (key.Length > MaxLength || key.Any(c => c is < '!' or > '~'))
            return ValidationErrors.Invalid(
                $"The {HeaderName} header must have at most {MaxLength} visible ASCII characters.");

        return Result.Success();
    }

    public static Error Reused() => Error.Failure(ReusedCode,
        $"The {HeaderName} was already used with a different request. Use a new key for a new transaction.");

    // Over the normalized values, so 12.3 and 12.30 or a padded CreatedBy are the same request.
    public static string Hash(CreateTransactionRequest request)
    {
        var canonical = string.Join('|',
            request.Type!.Value,
            request.AccountId!.Value.ToString(CultureInfo.InvariantCulture),
            request.Amount!.Value.ToString("F2", CultureInfo.InvariantCulture),
            request.CreatedBy!.Trim());
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
