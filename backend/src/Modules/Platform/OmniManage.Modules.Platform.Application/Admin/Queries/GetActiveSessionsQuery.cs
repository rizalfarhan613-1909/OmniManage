using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Common;

namespace OmniManage.Modules.Platform.Application.Admin.Queries;

public sealed record GetActiveSessionsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    string? Search = null) : IQuery<PagedResult<ActiveUserSessionDto>>;
