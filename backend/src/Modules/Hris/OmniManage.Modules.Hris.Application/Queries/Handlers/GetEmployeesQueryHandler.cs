using MediatR;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;
using OmniManage.Modules.Hris.Domain.Abstractions;

namespace OmniManage.Modules.Hris.Application.Queries;

public sealed class GetEmployeesQueryHandler(IEmployeeRepository employees)
    : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private static readonly IReadOnlyDictionary<string, EmployeeSortField> SortFields =
        new Dictionary<string, EmployeeSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["employeeCode"] = EmployeeSortField.EmployeeCode,
            ["fullName"] = EmployeeSortField.FullName,
            ["joinedDate"] = EmployeeSortField.JoinedDate,
        };

    public async Task<PagedResult<EmployeeDto>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, EmployeeSortField.EmployeeCode)
            .Select(option => new EmployeeSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new EmployeeListCriteria(
            skip,
            pageSize,
            request.Search?.Trim(),
            request.DepartmentId,
            request.IsActive,
            sort);

        var (found, totalItems) = await employees
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(EmployeeDto.From).ToArray();

        return new PagedResult<EmployeeDto>(dtos, page, pageSize, totalItems);
    }
}
