using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Domain.Errors;

namespace OmniManage.Modules.Sales.Domain.Orders;

public sealed class SalesOrderLine : Entity<Guid>
{
    public const decimal MaxQuantity = 999_999_999.999999m;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private SalesOrderLine()
    {
    }

    private SalesOrderLine(
        Guid id,
        Guid salesOrderId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice,
        decimal lineTotal)
        : base(id)
    {
        SalesOrderId = salesOrderId;
        ItemId = itemId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }

    public Guid SalesOrderId { get; private set; }

    public Guid ItemId { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal { get; private set; }

    internal static Result<SalesOrderLine> Create(
        Guid id,
        Guid salesOrderId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice)
    {
        if (id == Guid.Empty)
        {
            return SalesOrderErrors.InvalidLineId;
        }

        if (salesOrderId == Guid.Empty)
        {
            return SalesOrderErrors.InvalidId;
        }

        if (itemId == Guid.Empty)
        {
            return SalesOrderErrors.InvalidItemId;
        }

        if (quantity is <= 0m or > MaxQuantity)
        {
            return SalesOrderErrors.InvalidQuantity;
        }

        if (unitPrice is < 0m or > MaxAmount)
        {
            return SalesOrderErrors.InvalidUnitPrice;
        }

        decimal lineTotal;

        try
        {
            lineTotal = decimal.Round(quantity * unitPrice, 4, MidpointRounding.AwayFromZero);
        }
        catch (OverflowException)
        {
            return SalesOrderErrors.InvalidLineTotal;
        }

        if (lineTotal > MaxAmount)
        {
            return SalesOrderErrors.InvalidLineTotal;
        }

        return new SalesOrderLine(id, salesOrderId, itemId, quantity, unitPrice, lineTotal);
    }
}
