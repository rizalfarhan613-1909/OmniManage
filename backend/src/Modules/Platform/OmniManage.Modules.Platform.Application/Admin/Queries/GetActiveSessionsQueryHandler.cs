using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Common;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Admin.Queries;

public sealed class GetActiveSessionsQueryHandler(
    IRefreshTokenRepository refreshTokens,
    ITenantContext tenantContext,
    TimeProvider clock)
    : IQueryHandler<GetActiveSessionsQuery, PagedResult<ActiveUserSessionDto>>
{
    public async Task<PagedResult<ActiveUserSessionDto>> Handle(
        GetActiveSessionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.PageNumber, query.PageSize);
        var now = clock.GetUtcNow();

        var sessions = await refreshTokens
            .ListActiveSessionsAsync(tenantContext.TenantId, now, cancellationToken)
            .ConfigureAwait(false);

        IEnumerable<ActiveSessionProjection> filtered = sessions;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();

            filtered = filtered.Where(session =>
                session.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                || session.IpAddress.Contains(term, StringComparison.OrdinalIgnoreCase)
                || session.Roles.Any(role =>
                    role.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var ordered = filtered
            .OrderByDescending(session => session.LastSeenAt)
            .ThenBy(session => session.SessionId)
            .ToArray();

        var items = ordered
            .Skip(skip)
            .Take(pageSize)
            .Select(session => ActiveUserSessionDto.From(session, now))
            .ToArray();

        return new PagedResult<ActiveUserSessionDto>(items, page, pageSize, ordered.Length);
    }
}
