namespace OmniManage.Modules.Sales.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalItems)
{
    public int TotalPages => TotalItems == 0
        ? 0
        : (int)((TotalItems + PageSize - 1) / PageSize);
}
