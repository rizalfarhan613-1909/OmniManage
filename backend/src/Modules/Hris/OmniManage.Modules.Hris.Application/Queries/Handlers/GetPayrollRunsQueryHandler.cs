using MediatR;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Payroll;

namespace OmniManage.Modules.Hris.Application.Queries;

public sealed class GetPayrollRunsQueryHandler(IPayrollRunRepository payrollRuns)
    : IRequestHandler<GetPayrollRunsQuery, PagedResult<PayrollRunDto>>
{
    private static readonly IReadOnlyDictionary<string, PayrollRunSortField> SortFields =
        new Dictionary<string, PayrollRunSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["periodCode"] = PayrollRunSortField.PeriodCode,
            ["processedAtUtc"] = PayrollRunSortField.ProcessedAtUtc,
            ["netPayTotal"] = PayrollRunSortField.NetPayTotal,
        };

    public async Task<PagedResult<PayrollRunDto>> Handle(
        GetPayrollRunsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        PayrollStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<PayrollStatus>(request.Status, ignoreCase: true, out var parsedStatus)
            && Enum.IsDefined(parsedStatus))
        {
            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, PayrollRunSortField.PeriodCode)
            .Select(option => new PayrollRunSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new PayrollRunListCriteria(
            skip,
            pageSize,
            request.Search?.Trim(),
            request.PeriodCode,
            status,
            sort);

        var (found, totalItems) = await payrollRuns
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(PayrollRunDto.From).ToArray();

        return new PagedResult<PayrollRunDto>(dtos, page, pageSize, totalItems);
    }
}
