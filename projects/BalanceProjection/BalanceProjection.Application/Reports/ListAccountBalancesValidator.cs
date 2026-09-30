using BalanceProjection.Application.Results;
using BalanceProjection.Application.Validation;

namespace BalanceProjection.Application.Reports;

// Every rule of ListAccountBalancesRequest; reports all violations at once.
internal static class ListAccountBalancesValidator
{
    public static Result Validate(ListAccountBalancesRequest request)
    {
        List<Error> errors = [];

        if (request.Page < 1)
            errors.Add(ValidationErrors.Invalid("Page must be at least 1.", nameof(request.Page)));

        if (request.PageSize is < 1 or > ListAccountBalancesRequest.MaxPageSize)
            errors.Add(ValidationErrors.Invalid(
                $"PageSize must be between 1 and {ListAccountBalancesRequest.MaxPageSize}.", nameof(request.PageSize)));

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }
}
