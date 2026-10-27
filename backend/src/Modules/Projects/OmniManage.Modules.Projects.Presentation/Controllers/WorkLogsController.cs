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
[Route("api/v1/projects/work-logs")]
[RequireModule(ModuleCodes.ProjectCore)]
public sealed class WorkLogsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Projects.TaskRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? taskId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? sort = null)
    {
        var query = new GetWorkLogsQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            TaskId = taskId,
            EmployeeId = employeeId,
            FromDate = fromDate,
            ToDate = toDate,
            Sort = sort,
        };

        var result = await sender.Send(query, CancellationToken.None);
        return FromPagedResult(result);
    }
}
