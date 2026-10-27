using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;

namespace OmniManage.Modules.Projects.Application.Queries;

public class GetWorkLogsQuery : IRequest<PagedResult<WorkLogDto>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public Guid? TaskId { get; set; }

    public Guid? EmployeeId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public string? Sort { get; set; }
}
