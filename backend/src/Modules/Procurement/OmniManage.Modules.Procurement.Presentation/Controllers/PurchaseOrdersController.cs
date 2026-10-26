using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.Procurement.Application.Common;
using OmniManage.Modules.Procurement.Application.PurchaseOrders;
using OmniManage.Modules.Procurement.Application.PurchaseOrders.CreatePurchaseOrder;
using OmniManage.Modules.Procurement.Application.PurchaseOrders.GetPurchaseOrders;

namespace OmniManage.Modules.Procurement.Presentation.Controllers;

[ApiController]
[Route("api/v1/procurement/purchase-orders")]
[RequireModule(ModuleCodes.ProcCore)]
public sealed class PurchaseOrdersController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.Procurement.PurchaseOrderCreate)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lines = request.Lines?
            .Select(line => new CreatePurchaseOrderLine(line.ItemId, line.Quantity, line.UnitPrice))
            .ToArray();

        var result = await sender
            .Send(
                new CreatePurchaseOrderCommand(
                    request.OrderNumber,
                    request.SupplierId,
                    request.ExpectedDeliveryDate,
                    request.Notes,
                    lines),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(
            result,
            purchaseOrder => $"/api/v1/procurement/purchase-orders/{purchaseOrder.Id:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.Procurement.PurchaseOrderRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListPurchaseOrdersRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new GetPurchaseOrdersQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.Status,
                    request.SupplierId,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
