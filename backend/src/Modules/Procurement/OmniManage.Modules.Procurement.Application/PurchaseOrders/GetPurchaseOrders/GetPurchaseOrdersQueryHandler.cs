using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Application.Common;
using OmniManage.Modules.Procurement.Domain.Abstractions;
using OmniManage.Modules.Procurement.Domain.Errors;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Application.PurchaseOrders.GetPurchaseOrders;

public sealed class GetPurchaseOrdersQueryHandler(IPurchaseOrderRepository purchaseOrders)
    : IQueryHandler<GetPurchaseOrdersQuery, Result<PagedResult<PurchaseOrderDto>>>
{
    private static readonly IReadOnlyDictionary<string, PurchaseOrderSortField> SortFields =
        new Dictionary<string, PurchaseOrderSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["orderNumber"] = PurchaseOrderSortField.OrderNumber,
            ["supplierName"] = PurchaseOrderSortField.SupplierName,
            ["status"] = PurchaseOrderSortField.Status,
            ["totalAmount"] = PurchaseOrderSortField.TotalAmount,
            ["expectedDeliveryDate"] = PurchaseOrderSortField.ExpectedDeliveryDate,
            ["createdAt"] = PurchaseOrderSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<PurchaseOrderDto>>> Handle(
        GetPurchaseOrdersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        PurchaseOrderStatus? status = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<PurchaseOrderStatus>(query.Status, ignoreCase: true, out var parsedStatus)
                || !Enum.IsDefined(parsedStatus))
            {
                return PurchaseOrderErrors.InvalidStatusFilter;
            }

            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, PurchaseOrderSortField.CreatedAt)
            .Select(option => new PurchaseOrderSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new PurchaseOrderListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            status,
            query.SupplierId == Guid.Empty ? null : query.SupplierId,
            sort);

        var (orders, totalItems) = await purchaseOrders
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = orders.Select(PurchaseOrderDto.From).ToArray();

        return new PagedResult<PurchaseOrderDto>(dtos, page, pageSize, totalItems);
    }
}
