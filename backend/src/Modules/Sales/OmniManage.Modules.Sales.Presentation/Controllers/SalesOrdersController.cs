using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.Sales.Application.Common;
using OmniManage.Modules.Sales.Application.Orders;
using OmniManage.Modules.Sales.Application.Orders.ConfirmSalesOrder;
using OmniManage.Modules.Sales.Application.Orders.CreateSalesOrder;
using OmniManage.Modules.Sales.Application.Orders.GetSalesOrderById;
using OmniManage.Modules.Sales.Application.Orders.ListSalesOrders;

namespace OmniManage.Modules.Sales.Presentation.Controllers;

[ApiController]
[Route("api/v1/sales/orders")]
[RequireModule(ModuleCodes.SalesCore)]
public sealed class SalesOrdersController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.Sales.OrderCreate)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lines = request.Lines?
            .Select(line => new CreateSalesOrderLine(line.ItemId, line.Quantity, line.UnitPrice))
            .ToArray();

        var result = await sender
            .Send(
                new CreateSalesOrderCommand(request.OrderNumber, request.CustomerId, lines),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(result, salesOrder => $"/api/v1/sales/orders/{salesOrder.Id:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.Sales.OrderRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalesOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListSalesOrdersRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ListSalesOrdersQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.Status,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Sales.OrderRead)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new GetSalesOrderByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }

    [HttpPost("{id:guid}/confirm")]
    [HasPermission(Permissions.Sales.OrderConfirm)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new ConfirmSalesOrderCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }
}
