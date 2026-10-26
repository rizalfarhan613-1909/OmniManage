using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;

namespace OmniManage.Modules.Projects.Application.Queries;

public class GetProjectTasksQuery : IRequest<PagedResult<TaskDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}