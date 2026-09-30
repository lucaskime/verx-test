using BalanceProjection.Application.Results;
using BalanceProjection.Domain.Reports;
using BalanceProjection.Domain.Repositories;

namespace BalanceProjection.Application.Reports;

public sealed class ListAccountBalancesHandler(IBalanceReportRepository repository)
{
    public async Task<Result<IReadOnlyList<AccountBalanceReport>>> HandleAsync(
        ListAccountBalancesRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ListAccountBalancesValidator.Validate(request);
        if (validation.IsFailure)
            return Result.Failure<IReadOnlyList<AccountBalanceReport>>(validation.Errors);

        var reports = await repository.ListAsync(
            (request.Page - 1) * request.PageSize, request.PageSize, cancellationToken);
        return Result.Success(reports);
    }
}
