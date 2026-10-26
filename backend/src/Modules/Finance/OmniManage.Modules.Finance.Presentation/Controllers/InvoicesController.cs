using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Finance.Application.Common;
using OmniManage.Modules.Finance.Application.Invoices;
using OmniManage.Modules.Finance.Application.Invoices.ListArInvoices;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Finance.Presentation.Controllers;

[ApiController]
[Route("api/v1/finance/invoices")]
[RequireModule(ModuleCodes.FinAr)]
public sealed class InvoicesController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Finance.InvoiceRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ArInvoiceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] ListArInvoicesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ListArInvoicesQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.Status,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
