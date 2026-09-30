using Transaction.Application.Results;
using Transaction.Application.Validation;
using Transaction.Domain.Transactions;

namespace Transaction.Application.Transactions;

// Every rule of CreateTransactionRequest; reports all violations at once.
internal static class CreateTransactionValidator
{
    public const int CreatedByMaxLength = 100;
    private const decimal MaxAmount = 9999999999999999.99m; // decimal(18,2)

    public static Result Validate(CreateTransactionRequest request)
    {
        List<Error> errors = [];

        if (request.Type is not { } type)
            errors.Add(Required(nameof(request.Type)));
        else if (!Enum.IsDefined(type))
            errors.Add(ValidationErrors.Invalid("Type must be Debit or Credit.", nameof(request.Type)));

        if (request.AccountId is null or <= 0)
            errors.Add(Required(nameof(request.AccountId)));

        if (request.Amount is not { } amount)
            errors.Add(Required(nameof(request.Amount)));
        else if (amount <= 0 || amount > MaxAmount || decimal.Round(amount, 2) != amount)
            errors.Add(ValidationErrors.Invalid(
                "Amount must be positive, fit decimal(18,2), and have at most two decimal places.", nameof(request.Amount)));

        if (string.IsNullOrWhiteSpace(request.CreatedBy))
            errors.Add(Required(nameof(request.CreatedBy)));
        else if (request.CreatedBy.Trim().Length > CreatedByMaxLength)
            errors.Add(ValidationErrors.Invalid(
                $"CreatedBy must have at most {CreatedByMaxLength} characters.", nameof(request.CreatedBy)));

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }

    private static Error Required(string field) => ValidationErrors.Invalid($"{field} is required.", field);
}
