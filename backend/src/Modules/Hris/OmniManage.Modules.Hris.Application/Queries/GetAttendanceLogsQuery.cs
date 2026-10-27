using MediatR;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;

namespace OmniManage.Modules.Hris.Application.Queries;

public class GetAttendanceLogsQuery : IRequest<PagedResult<AttendanceLogDto>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public Guid? EmployeeId { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? FromUtc { get; set; }

    public DateTimeOffset? ToUtc { get; set; }

    public string? Sort { get; set; }
}
