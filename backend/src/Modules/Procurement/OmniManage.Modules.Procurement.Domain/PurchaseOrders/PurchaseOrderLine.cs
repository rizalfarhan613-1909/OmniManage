using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Domain.Errors;

namespace OmniManage.Modules.Procurement.Domain.PurchaseOrders;

public sealed class PurchaseOrderLine : Entity<Guid>
{
    public const decimal MaxQuantity = 999_999_999.999999m;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private PurchaseOrderLine()
    {
    }

    private PurchaseOrderLine(
        Guid id,
        Guid purchaseOrderId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice,
        decimal lineTotal)
        : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        ItemId = itemId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }

    public Guid PurchaseOrderId { get; private set; }

    public Guid ItemId { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal { get; private set; }

    internal static Result<PurchaseOrderLine> Create(
        Guid id,
        Guid purchaseOrderId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice)
    {
        if (id == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidLineId;
        }

        if (purchaseOrderId == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidId;
        }

        if (itemId == Guid.Empty)
        {
            return PurchaseOrderErrors.InvalidItemId;
        }

        if (quantity is <= 0m or > MaxQuantity)
        {
            return PurchaseOrderErrors.InvalidQuantity;
        }

        if (unitPrice is < 0m or > MaxAmount)
        {
            return PurchaseOrderErrors.InvalidUnitPrice;
        }

        decimal lineTotal;

        try
        {
            lineTotal = decimal.Round(quantity * unitPrice, 4, MidpointRounding.AwayFromZero);
        }
        catch (OverflowException)
        {
            return PurchaseOrderErrors.InvalidLineTotal;
        }

        if (lineTotal > MaxAmount)
        {
            return PurchaseOrderErrors.InvalidLineTotal;
        }

        return new PurchaseOrderLine(id, purchaseOrderId, itemId, quantity, unitPrice, lineTotal);
    }
}
