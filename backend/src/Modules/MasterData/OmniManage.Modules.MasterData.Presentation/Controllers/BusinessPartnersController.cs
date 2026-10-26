using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.MasterData.Application.BusinessPartners;
using OmniManage.Modules.MasterData.Application.BusinessPartners.CreateBusinessPartner;
using OmniManage.Modules.MasterData.Application.BusinessPartners.ListBusinessPartners;
using OmniManage.Modules.MasterData.Application.Common;
using OmniManage.Modules.Platform.Contracts.Authorization;

namespace OmniManage.Modules.MasterData.Presentation.Controllers;

[ApiController]
[Route("api/v1/masterdata/business-partners")]
[RequireModule(ModuleCodes.CoreMasterData)]
public sealed class BusinessPartnersController(ISender sender, TimeProvider clock)
    : ApiControllerBase(clock)
{
    [HttpPost]
    [HasPermission(Permissions.MasterData.BusinessPartnerCreate)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBusinessPartnerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new CreateBusinessPartnerCommand(
                    request.Code,
                    request.Name,
                    request.PartnerType,
                    request.TaxId,
                    request.CreditLimit),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(
            result,
            businessPartner => $"/api/v1/masterdata/business-partners/{businessPartner.Id:D}");
    }

    [HttpGet]
    [HasPermission(Permissions.MasterData.BusinessPartnerRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BusinessPartnerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListBusinessPartnersRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ListBusinessPartnersQuery(request.Page, request.PageSize, request.Search, request.Sort),
                cancellationToken)
            .ConfigureAwait(false);

        return FromPagedResult(result);
    }
}
