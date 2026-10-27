using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.Projects.Application.Common;
using OmniManage.Modules.Projects.Application.Dtos;
using OmniManage.Modules.Projects.Application.Queries;

namespace OmniManage.Modules.Projects.Presentation.Controllers;

[ApiController]
[Route("api/v1/projects")]
[RequireModule(ModuleCodes.ProjectCore)]
public sealed class ProjectsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Projects.TaskRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sort = null)
    {
        var query = new GetProjectsQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Search = search,
            CustomerId = customerId,
            Status = status,
            Sort = sort,
        };

        var result = await sender.Send(query, CancellationToken.None);
        return FromPagedResult(result);
    }
}
