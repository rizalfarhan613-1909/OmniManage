using MediatR;
using OmniManage.Modules.AssetMaintenance.Application.Common;
using OmniManage.Modules.AssetMaintenance.Application.Dtos;
using OmniManage.Modules.AssetMaintenance.Application.Queries;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Application.Handlers;

public sealed class GetMaintenanceOrdersQueryHandler(IMaintenanceOrderRepository maintenanceOrders)
    : IRequestHandler<GetMaintenanceOrdersQuery, PagedResult<MaintenanceOrderDto>>
{
    private static readonly IReadOnlyDictionary<string, MaintenanceOrderSortField> SortFields =
        new Dictionary<string, MaintenanceOrderSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["requestNumber"] = MaintenanceOrderSortField.RequestNumber,
            ["scheduledDate"] = MaintenanceOrderSortField.ScheduledDate,
            ["cost"] = MaintenanceOrderSortField.Cost,
        };

    public async Task<PagedResult<MaintenanceOrderDto>> Handle(
        GetMaintenanceOrdersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (page, pageSize, skip) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var sort = SortParser
            .Parse(request.Sort, SortFields, MaintenanceOrderSortField.ScheduledDate)
            .Select(option => new MaintenanceOrderSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new MaintenanceOrderListCriteria(
            skip,
            pageSize,
            request.AssetId,
            ParseStatus(request.Status),
            sort);

        var (found, totalItems) = await maintenanceOrders
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(MaintenanceOrderDto.From).ToArray();

        return new PagedResult<MaintenanceOrderDto>(dtos, page, pageSize, totalItems);
    }

    private static MaintenanceStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Enum.TryParse<MaintenanceStatus>(status.Trim(), ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }
}
