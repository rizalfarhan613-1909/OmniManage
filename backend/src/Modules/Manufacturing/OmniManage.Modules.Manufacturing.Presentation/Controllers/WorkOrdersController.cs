using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Manufacturing.Application.Common;
using OmniManage.Modules.Manufacturing.Application.WorkOrders;
using OmniManage.Modules.Manufacturing.Application.WorkOrders.CreateWorkOrder;
using OmniManage.Modules.Manufacturing.Application.WorkOrders.GetWorkOrders;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Manufacturing.Presentation.Controllers;

[ApiController]
[Route("api/v1/manufacturing/work-orders")]
[RequireModule(ModuleCodes.MfgCore)]
public sealed class WorkOrdersController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.Manufacturing.WorkOrderCreate)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkOrderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new CreateWorkOrderCommand(
                    request.OrderNumber,
                    request.BillOfMaterialId,
                    request.QuantityPlanned,
                    request.DueDate,
                    request.Notes),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(
            result,
            workOrder => $"/api/v1/manufacturing/work-orders/{workOrder.Id:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.Manufacturing.WorkOrderRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListWorkOrdersRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new GetWorkOrdersQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.Status,
                    request.ProductItemId,
                    request.BillOfMaterialId,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
