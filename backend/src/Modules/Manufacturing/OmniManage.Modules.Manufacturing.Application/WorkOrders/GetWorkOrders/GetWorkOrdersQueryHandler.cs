using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Application.Common;
using OmniManage.Modules.Manufacturing.Domain.Abstractions;
using OmniManage.Modules.Manufacturing.Domain.Errors;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Application.WorkOrders.GetWorkOrders;

public sealed class GetWorkOrdersQueryHandler(IWorkOrderRepository workOrders)
    : IQueryHandler<GetWorkOrdersQuery, Result<PagedResult<WorkOrderDto>>>
{
    private static readonly IReadOnlyDictionary<string, WorkOrderSortField> SortFields =
        new Dictionary<string, WorkOrderSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["orderNumber"] = WorkOrderSortField.OrderNumber,
            ["status"] = WorkOrderSortField.Status,
            ["productItemId"] = WorkOrderSortField.ProductItemId,
            ["quantityPlanned"] = WorkOrderSortField.QuantityPlanned,
            ["dueDate"] = WorkOrderSortField.DueDate,
            ["createdAt"] = WorkOrderSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<WorkOrderDto>>> Handle(
        GetWorkOrdersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        WorkOrderStatus? status = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<WorkOrderStatus>(query.Status, ignoreCase: true, out var parsedStatus)
                || !Enum.IsDefined(parsedStatus))
            {
                return WorkOrderErrors.InvalidStatusFilter;
            }

            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, WorkOrderSortField.CreatedAt)
            .Select(option => new WorkOrderSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new WorkOrderListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            status,
            query.ProductItemId == Guid.Empty ? null : query.ProductItemId,
            query.BillOfMaterialId == Guid.Empty ? null : query.BillOfMaterialId,
            sort);

        var (items, totalItems) = await workOrders
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(WorkOrderDto.From).ToArray();

        return new PagedResult<WorkOrderDto>(dtos, page, pageSize, totalItems);
    }
}
