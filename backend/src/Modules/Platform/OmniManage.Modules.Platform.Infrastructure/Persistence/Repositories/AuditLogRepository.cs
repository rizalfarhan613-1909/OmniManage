using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(PlatformDbContext dbContext) : IAuditLogRepository
{
    public async Task<(IReadOnlyList<AuditLogProjection> Items, long TotalItems)> ListAsync(
        Guid? tenantId,
        AuditLogListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = dbContext.AuditLogs.AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(log => log.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Action))
        {
            var action = criteria.Action.Trim().ToLowerInvariant();

            query = query.Where(log => log.Action.ToLower().Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(criteria.ModuleCode))
        {
            var moduleCode = criteria.ModuleCode.Trim().ToUpperInvariant();

            query = query.Where(log => log.ModuleCode == moduleCode);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(log =>
                log.Action.ToLower().Contains(term)
                || (log.EntityType != null && log.EntityType.ToLower().Contains(term))
                || (log.EntityId != null && log.EntityId.ToLower().Contains(term))
                || (log.TraceId != null && log.TraceId.ToLower().Contains(term)));
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var page = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(log => new
            {
                log.Id,
                log.TenantId,
                log.OccurredAt,
                log.ActorId,
                log.ActorType,
                log.Action,
                log.ModuleCode,
                log.EntityType,
                log.EntityId,
                log.BeforeData,
                log.AfterData,
                log.IpAddress,
                log.UserAgent,
                log.TraceId,
                log.RowHash,
                log.PrevHash,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var actorIds = page
            .Where(log => log.ActorId.HasValue)
            .Select(log => log.ActorId!.Value)
            .Distinct()
            .ToArray();

        var actorNames = actorIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.Users
                .AsNoTracking()
                .Where(user => actorIds.Contains(user.Id))
                .ToDictionaryAsync(
                    user => user.Id,
                    user => user.Email,
                    cancellationToken)
                .ConfigureAwait(false);

        var items = page
            .Select(log => new AuditLogProjection(
                log.Id,
                log.TenantId,
                log.OccurredAt,
                log.ActorId,
                log.ActorId.HasValue && actorNames.TryGetValue(log.ActorId.Value, out var name)
                    ? name
                    : null,
                log.ActorType,
                log.Action,
                log.ModuleCode,
                log.EntityType,
                log.EntityId,
                log.BeforeData,
                log.AfterData,
                log.IpAddress?.ToString(),
                log.UserAgent,
                log.TraceId,
                log.RowHash,
                log.PrevHash))
            .ToArray();

        return (items, totalItems);
    }

    private static IQueryable<AuditLog> ApplySort(
        IQueryable<AuditLog> query,
        AuditLogListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new AuditLogSortOption(AuditLogSortField.OccurredAt, true)
            : criteria.Sort[0];

        IOrderedQueryable<AuditLog> ordered =
            option.Field switch
            {
                AuditLogSortField.Action => option.Descending
                    ? query.OrderByDescending(log => log.Action)
                    : query.OrderBy(log => log.Action),
                AuditLogSortField.OccurredAt => option.Descending
                    ? query.OrderByDescending(log => log.OccurredAt)
                    : query.OrderBy(log => log.OccurredAt),
                _ => query.OrderByDescending(log => log.OccurredAt),
            };

        return ordered.ThenByDescending(log => log.Id);
    }
}
