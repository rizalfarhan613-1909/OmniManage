using System.Globalization;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Application.Orders;

public sealed record SalesOrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    string TotalAmount,
    IReadOnlyList<SalesOrderLineDto> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static SalesOrderDto From(SalesOrder salesOrder)
    {
        ArgumentNullException.ThrowIfNull(salesOrder);

        return new SalesOrderDto(
            salesOrder.Id,
            salesOrder.OrderNumber,
            salesOrder.CustomerId,
            salesOrder.Status.ToString(),
            FormatAmount(salesOrder.TotalAmount),
            salesOrder.Lines.Select(SalesOrderLineDto.From).ToArray(),
            salesOrder.CreatedAt,
            salesOrder.UpdatedAt);
    }

    internal static string FormatAmount(decimal amount) =>
        amount.ToString("0.0000", CultureInfo.InvariantCulture);
}

public sealed record SalesOrderLineDto(
    Guid Id,
    Guid ItemId,
    string Quantity,
    string UnitPrice,
    string LineTotal)
{
    internal static SalesOrderLineDto From(SalesOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new SalesOrderLineDto(
            line.Id,
            line.ItemId,
            line.Quantity.ToString("0.0000", CultureInfo.InvariantCulture),
            SalesOrderDto.FormatAmount(line.UnitPrice),
            SalesOrderDto.FormatAmount(line.LineTotal));
    }
}
