using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Inventory.Application.Common;
using OmniManage.Modules.Inventory.Application.Stock;
using OmniManage.Modules.Inventory.Application.Stock.AdjustStock;
using OmniManage.Modules.Inventory.Application.Stock.GetStockBalances;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Inventory.Presentation.Controllers;

[ApiController]
[Route("api/v1/inventory/stock")]
[RequireModule(ModuleCodes.InvCore)]
public sealed class StockController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost("adjustments")]
    [HasPermission(Permissions.Inventory.StockAdjust)]
    [ProducesResponseType(typeof(ApiResponse<StockEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new AdjustStockCommand(
                    request.WarehouseId,
                    request.ItemId,
                    request.QuantityDelta,
                    request.MovementType,
                    request.ReferenceNumber,
                    request.OccurredAtUtc),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(result, entry => $"/api/v1/inventory/stock/ledger/{entry.Id:D}");
    }

    [HttpGet("balances")]
    [HasPermission(Permissions.Inventory.StockRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockBalanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalances(
        [FromQuery] GetStockBalancesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new GetStockBalancesQuery(
                    request.Page,
                    request.PageSize,
                    request.WarehouseId,
                    request.ItemId),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
