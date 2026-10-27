using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;
using OmniManage.Modules.Projects.Domain.Abstractions;

namespace OmniManage.Modules.Projects.Application.Queries;

public sealed class GetWorkLogsQueryHandler(IWorkLogRepository workLogs)
    : IRequestHandler<GetWorkLogsQuery, PagedResult<WorkLogDto>>
{
    private static readonly IReadOnlyDictionary<string, WorkLogSortField> SortFields =
        new Dictionary<string, WorkLogSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["loggedDate"] = WorkLogSortField.LoggedDate,
            ["hoursSpent"] = WorkLogSortField.HoursSpent,
        };

    public async Task<PagedResult<WorkLogDto>> Handle(
        GetWorkLogsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, WorkLogSortField.LoggedDate)
            .Select(option => new WorkLogSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new WorkLogListCriteria(
            skip,
            pageSize,
            request.TaskId,
            request.EmployeeId,
            request.FromDate,
            request.ToDate,
            sort);

        var (found, totalItems) = await workLogs
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(WorkLogDto.From).ToArray();

        return new PagedResult<WorkLogDto>(dtos, page, pageSize, totalItems);
    }
}
