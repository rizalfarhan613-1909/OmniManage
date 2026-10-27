using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Application.Admin.Commands;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Admin.Queries;
using OmniManage.Modules.Platform.Application.Common;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Platform.Presentation.Controllers;

[ApiController]
[Route("api/v1/platform/sessions")]
[RequireModule(ModuleCodes.PlatformCore)]
public sealed class SessionsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Platform.SessionRead)]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<ActiveUserSessionDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender
            .Send(new GetActiveSessionsQuery(page, pageSize, search), cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }

    [HttpDelete("{sessionId:guid}")]
    [HasPermission(Permissions.Platform.SessionTerminate)]
    [ProducesResponseType(
        typeof(ApiResponse<TerminateSessionResultDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Terminate(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new TerminateSessionCommand(sessionId), cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }
}
