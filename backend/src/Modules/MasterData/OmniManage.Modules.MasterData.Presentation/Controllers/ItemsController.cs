using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.MasterData.Application.Common;
using OmniManage.Modules.MasterData.Application.Items;
using OmniManage.Modules.MasterData.Application.Items.CreateItem;
using OmniManage.Modules.MasterData.Application.Items.ListItems;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.MasterData.Presentation.Controllers;

[ApiController]
[Route("api/v1/masterdata/items")]
[RequireModule(ModuleCodes.CoreMasterData)]
public sealed class ItemsController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.MasterData.ItemCreate)]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new CreateItemCommand(
                    request.Sku,
                    request.Name,
                    request.UomCode,
                    request.Category,
                    request.BasePrice,
                    request.CostPrice,
                    request.IsStockTracked),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(result, item => $"/api/v1/masterdata/items/{item.Id:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.MasterData.ItemRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListItemsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(new ListItemsQuery(request.Page, request.PageSize, request.Search, request.Sort), cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
