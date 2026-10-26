using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Manufacturing.Application.BillOfMaterials;
using OmniManage.Modules.Manufacturing.Application.BillOfMaterials.GetBillOfMaterials;
using OmniManage.Modules.Manufacturing.Application.Common;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Manufacturing.Presentation.Controllers;

[ApiController]
[Route("api/v1/manufacturing/bom")]
[RequireModule(ModuleCodes.MfgCore)]
public sealed class BillOfMaterialsController(ISender sender, TimeProvider clock)
    : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Manufacturing.BillOfMaterialRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BillOfMaterialDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListBillOfMaterialsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new GetBillOfMaterialsQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.ProductItemId,
                    request.IsActive,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
