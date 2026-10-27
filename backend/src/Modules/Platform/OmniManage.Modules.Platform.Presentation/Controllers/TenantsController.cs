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
[Route("api/v1/platform/tenants")]
[RequireModule(ModuleCodes.PlatformCore)]
public sealed class TenantsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Platform.TenantRead)]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<TenantSummaryDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender
            .Send(new GetTenantsQuery(page, pageSize, search, sort), cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }

    [HttpPatch("{tenantId:guid}")]
    [HasPermission(Permissions.Platform.TenantManage)]
    [ProducesResponseType(typeof(ApiResponse<TenantStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(new UpdateTenantStatusCommand(tenantId, request.Status), cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }

    [HttpPatch("{tenantId:guid}/modules/{code}")]
    [HasPermission(Permissions.Platform.TenantManage)]
    [ProducesResponseType(typeof(ApiResponse<TenantModuleLicenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ToggleModule(
        Guid tenantId,
        string code,
        [FromBody] ToggleTenantModuleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ToggleTenantModuleCommand(tenantId, code, request.IsActive, request.Reason),
                cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }
}
