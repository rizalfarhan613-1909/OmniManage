using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Application.Common;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Application.Orders.ListSalesOrders;

public sealed class ListSalesOrdersQueryHandler(ISalesOrderRepository salesOrders)
    : IQueryHandler<ListSalesOrdersQuery, Result<PagedResult<SalesOrderDto>>>
{
    private static readonly IReadOnlyDictionary<string, SalesOrderSortField> SortFields =
        new Dictionary<string, SalesOrderSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["orderNumber"] = SalesOrderSortField.OrderNumber,
            ["status"] = SalesOrderSortField.Status,
            ["totalAmount"] = SalesOrderSortField.TotalAmount,
            ["createdAt"] = SalesOrderSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<SalesOrderDto>>> Handle(
        ListSalesOrdersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        SalesOrderStatus? status = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<SalesOrderStatus>(query.Status, ignoreCase: true, out var parsedStatus)
                || !Enum.IsDefined(parsedStatus))
            {
                return SalesOrderErrors.InvalidStatusFilter;
            }

            status = parsedStatus;
        }

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, SalesOrderSortField.CreatedAt)
            .Select(option => new SalesOrderSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new SalesOrderListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            status,
            sort);

        var (orders, totalItems) = await salesOrders
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = orders.Select(SalesOrderDto.From).ToArray();

        return new PagedResult<SalesOrderDto>(dtos, page, pageSize, totalItems);
    }
}
