using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Common;

namespace OmniManage.Modules.Platform.Application.Admin.Queries;

public sealed record GetTenantsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    string? Search = null,
    string? Sort = null) : IQuery<PagedResult<TenantSummaryDto>>;
