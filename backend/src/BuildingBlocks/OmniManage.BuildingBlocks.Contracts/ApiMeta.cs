namespace OmniManage.BuildingBlocks.Contracts;

public sealed record ApiMeta(string TraceId, DateTime Timestamp, PaginationMeta? Pagination = null);
