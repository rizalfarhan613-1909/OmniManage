using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;
using OmniManage.Modules.Projects.Application.Queries;

namespace OmniManage.Modules.Projects.Application.Handlers;

public class GetProjectTasksQueryHandler : IRequestHandler<GetProjectTasksQuery, PagedResult<TaskDto>>
{
    public Task<PagedResult<TaskDto>> Handle(GetProjectTasksQuery request, CancellationToken cancellationToken)
    {
        // In a real implementation, this would query the database through a repository
        // For now, returning a placeholder result
        return Task.FromResult(new PagedResult<TaskDto>(
            Items: new List<TaskDto>(),
            Page: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: 0
        ));
    }
}