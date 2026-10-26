namespace OmniManage.BuildingBlocks.Contracts;

public sealed record PaginationMeta(int Page, int PageSize, long TotalItems, int TotalPages);
