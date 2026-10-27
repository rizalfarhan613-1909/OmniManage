using MediatR;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Attendance;

namespace OmniManage.Modules.Hris.Application.Queries;

public sealed class GetAttendanceLogsQueryHandler(IAttendanceLogRepository attendanceLogs)
    : IRequestHandler<GetAttendanceLogsQuery, PagedResult<AttendanceLogDto>>
{
    private static readonly IReadOnlyDictionary<string, AttendanceLogSortField> SortFields =
        new Dictionary<string, AttendanceLogSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["clockInUtc"] = AttendanceLogSortField.ClockInUtc,
            ["employeeId"] = AttendanceLogSortField.EmployeeId,
            ["status"] = AttendanceLogSortField.Status,
        };

    public async Task<PagedResult<AttendanceLogDto>> Handle(
        GetAttendanceLogsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        AttendanceStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<AttendanceStatus>(request.Status, ignoreCase: true, out var parsedStatus)
            && Enum.IsDefined(parsedStatus))
        {
            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, AttendanceLogSortField.ClockInUtc)
            .Select(option => new AttendanceLogSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new AttendanceLogListCriteria(
            skip,
            pageSize,
            request.EmployeeId,
            status,
            request.FromUtc,
            request.ToUtc,
            sort);

        var (logs, totalItems) = await attendanceLogs
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = logs.Select(AttendanceLogDto.From).ToArray();

        return new PagedResult<AttendanceLogDto>(dtos, page, pageSize, totalItems);
    }
}
