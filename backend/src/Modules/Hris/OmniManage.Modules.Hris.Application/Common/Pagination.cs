namespace OmniManage.Modules.Hris.Application.Common;

public static class Pagination
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize, int Skip) Normalize(int page, int pageSize)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var skip = (int)Math.Min((long)(normalizedPage - 1) * normalizedPageSize, int.MaxValue);

        return (normalizedPage, normalizedPageSize, skip);
    }
}
