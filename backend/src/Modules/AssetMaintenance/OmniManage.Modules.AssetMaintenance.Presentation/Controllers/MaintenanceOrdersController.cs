using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.AssetMaintenance.Application.Common;
using OmniManage.Modules.AssetMaintenance.Application.Dtos;
using OmniManage.Modules.AssetMaintenance.Application.Queries;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.AssetMaintenance.Presentation.Controllers;

[ApiController]
[Route("api/v1/assets/maintenance-orders")]
[RequireModule(ModuleCodes.AssetCore)]
public sealed class MaintenanceOrdersController(ISender sender, TimeProvider clock)
    : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Assets.MaintenanceRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<MaintenanceOrderDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? assetId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sort = null)
    {
        var query = new GetMaintenanceOrdersQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            AssetId = assetId,
            Status = status,
            Sort = sort,
        };

        var result = await sender.Send(query, CancellationToken.None);
        return FromPagedResult(result);
    }
}
