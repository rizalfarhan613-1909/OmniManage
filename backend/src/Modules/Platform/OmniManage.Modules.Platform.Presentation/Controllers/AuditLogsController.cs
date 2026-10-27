using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Admin.Queries;
using OmniManage.Modules.Platform.Application.Common;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Platform.Presentation.Controllers;

[ApiController]
[Route("api/v1/platform/audit-logs")]
[RequireModule(ModuleCodes.PlatformCore)]
public sealed class AuditLogsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Platform.AuditRead)]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<AuditLogEntryDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? action = null,
        [FromQuery] string? moduleCode = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender
            .Send(
                new GetAuditLogsQuery(page, pageSize, search, action, moduleCode, sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
