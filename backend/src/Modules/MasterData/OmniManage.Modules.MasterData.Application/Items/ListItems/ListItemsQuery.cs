using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Application.Common;

namespace OmniManage.Modules.MasterData.Application.Items.ListItems;

public sealed record ListItemsQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    string? Sort = null) : IQuery<Result<PagedResult<ItemDto>>>;
