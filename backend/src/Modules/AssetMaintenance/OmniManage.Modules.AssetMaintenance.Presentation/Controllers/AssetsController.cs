using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.AssetMaintenance.Application.Assets.CreateAsset;
using OmniManage.Modules.AssetMaintenance.Application.Common;
using OmniManage.Modules.AssetMaintenance.Application.Dtos;
using OmniManage.Modules.AssetMaintenance.Application.Maintenance.RequestMaintenance;
using OmniManage.Modules.AssetMaintenance.Application.Queries;

namespace OmniManage.Modules.AssetMaintenance.Presentation.Controllers;

[ApiController]
[Route("api/v1/assets")]
[RequireModule(ModuleCodes.AssetCore)]
public sealed class AssetsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.Assets.AssetCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new CreateAssetCommand(
                    request.AssetCode,
                    request.Name,
                    request.Category,
                    request.SerialNumber,
                    request.QrCodeUrl,
                    request.PurchaseValue,
                    request.AcquisitionDate),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(result, assetId => $"/api/v1/assets/{assetId:D}");
    }

    [HttpPost("{assetId:guid}/maintenance-orders")]
    [HasPermission(Permissions.Assets.MaintenanceRequest)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestMaintenance(
        Guid assetId,
        [FromBody] CreateMaintenanceOrderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new RequestMaintenanceCommand(
                    assetId,
                    request.RequestNumber,
                    request.ScheduledDate,
                    request.Description,
                    request.MaintenanceType,
                    request.Priority),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(
            result,
            maintenanceOrderId =>
                $"/api/v1/assets/{assetId:D}/maintenance-orders/{maintenanceOrderId:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.Assets.AssetRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sort = null)
    {
        var query = new GetAssetsQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Search = search,
            Category = category,
            Status = status,
            Sort = sort,
        };

        var result = await sender.Send(query, CancellationToken.None);
        return FromPagedResult(result);
    }
}
