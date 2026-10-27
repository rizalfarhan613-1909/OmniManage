using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Common;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Admin.Queries;

public sealed class GetAuditLogsQueryHandler(
    IAuditLogRepository auditLogs,
    ITenantContext tenantContext)
    : IQueryHandler<GetAuditLogsQuery, PagedResult<AuditLogEntryDto>>
{
    private static readonly IReadOnlyDictionary<string, AuditLogSortField> SortFields =
        new Dictionary<string, AuditLogSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["occurredAt"] = AuditLogSortField.OccurredAt,
            ["action"] = AuditLogSortField.Action,
        };

    public async Task<PagedResult<AuditLogEntryDto>> Handle(
        GetAuditLogsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.PageNumber, query.PageSize);

        var sort = string.IsNullOrWhiteSpace(query.Sort)
            ? [new AuditLogSortOption(AuditLogSortField.OccurredAt, true)]
            : SortParser
                .Parse(query.Sort, SortFields, AuditLogSortField.OccurredAt)
                .Select(option => new AuditLogSortOption(option.Field, option.Descending))
                .ToArray();

        var criteria = new AuditLogListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            query.Action?.Trim(),
            query.ModuleCode?.Trim(),
            sort);

        var (found, totalItems) = await auditLogs
            .ListAsync(tenantContext.TenantId, criteria, cancellationToken)
            .ConfigureAwait(false);

        var items = found.Select(AuditLogEntryDto.From).ToArray();

        return new PagedResult<AuditLogEntryDto>(items, page, pageSize, totalItems);
    }
}
