using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.Procurement.Application.Common;
using OmniManage.Modules.Procurement.Application.Suppliers;
using OmniManage.Modules.Procurement.Application.Suppliers.GetSuppliers;

namespace OmniManage.Modules.Procurement.Presentation.Controllers;

[ApiController]
[Route("api/v1/procurement/suppliers")]
[RequireModule(ModuleCodes.ProcCore)]
public sealed class SuppliersController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Procurement.SupplierRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListSuppliersRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new GetSuppliersQuery(
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
