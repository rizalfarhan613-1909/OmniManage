using MediatR;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;

namespace OmniManage.Modules.Projects.Application.Queries;

public class GetProjectsQuery : IRequest<PagedResult<ProjectDto>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public Guid? CustomerId { get; set; }

    public string? Status { get; set; }

    public string? Sort { get; set; }
}
