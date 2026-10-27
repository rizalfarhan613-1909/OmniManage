using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Hris.Application.Common;
using OmniManage.Modules.Hris.Application.Dtos;
using OmniManage.Modules.Hris.Application.Queries;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Hris.Presentation.Controllers;

[ApiController]
[Route("api/v1/hris/employees")]
[RequireModule(ModuleCodes.HrisCore)]
public sealed class EmployeesController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Hris.EmployeeViewSensitive)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sort = null)
    {
        var query = new GetEmployeesQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Search = search,
            DepartmentId = departmentId,
            IsActive = isActive,
            Sort = sort,
        };

        var result = await sender.Send(query, CancellationToken.None);
        return FromPagedResult(result);
    }
}
