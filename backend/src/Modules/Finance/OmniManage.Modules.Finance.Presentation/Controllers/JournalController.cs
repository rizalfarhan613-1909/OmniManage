using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Finance.Application.Common;
using OmniManage.Modules.Finance.Application.Journal;
using OmniManage.Modules.Finance.Application.Journal.ListJournalEntries;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.Finance.Presentation.Controllers;

[ApiController]
[Route("api/v1/finance/journal-entries")]
[RequireModule(ModuleCodes.FinGl)]
public sealed class JournalController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpGet]
    [HasPermission(Permissions.Finance.JournalRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<JournalEntryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] ListJournalEntriesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ListJournalEntriesQuery(
                    request.Page,
                    request.PageSize,
                    request.Search,
                    request.PostedFromUtc,
                    request.PostedToUtc,
                    request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
