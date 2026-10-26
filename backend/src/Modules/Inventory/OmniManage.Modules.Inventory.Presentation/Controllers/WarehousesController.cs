using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Inventory.Application.Common;
using OmniManage.Modules.Inventory.Application.Warehouses;
using OmniManage.Modules.Inventory.Application.Warehouses.CreateWarehouse;
using OmniManage.Modules.Inventory.Application.Warehouses.ListWarehouses;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Inventory.Presentation.Controllers;

[ApiController]
[Route("api/v1/inventory/warehouses")]
[RequireModule(ModuleCodes.InvCore)]
public sealed class WarehousesController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.Inventory.WarehouseCreate)]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(new CreateWarehouseCommand(request.Code, request.Name, request.Address), cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(result, warehouse => $"/api/v1/inventory/warehouses/{warehouse.Id:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.Inventory.WarehouseRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WarehouseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListWarehousesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ListWarehousesQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.IsActive,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
