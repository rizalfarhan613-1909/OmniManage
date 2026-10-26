using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Domain.Errors;

namespace OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

public sealed class BillOfMaterialComponent : Entity<Guid>
{
    public const decimal MaxQuantityPerUnit = 999_999_999.999999m;

    private BillOfMaterialComponent()
    {
    }

    private BillOfMaterialComponent(
        Guid id,
        Guid billOfMaterialId,
        Guid itemId,
        decimal quantityPerUnit)
        : base(id)
    {
        BillOfMaterialId = billOfMaterialId;
        ItemId = itemId;
        QuantityPerUnit = quantityPerUnit;
    }

    public Guid BillOfMaterialId { get; private set; }

    public Guid ItemId { get; private set; }

    public decimal QuantityPerUnit { get; private set; }

    internal static Result<BillOfMaterialComponent> Create(
        Guid id,
        Guid billOfMaterialId,
        Guid itemId,
        decimal quantityPerUnit)
    {
        if (id == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidComponentId;
        }

        if (billOfMaterialId == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidId;
        }

        if (itemId == Guid.Empty)
        {
            return BillOfMaterialErrors.InvalidComponentItemId;
        }

        if (quantityPerUnit is <= 0m or > MaxQuantityPerUnit)
        {
            return BillOfMaterialErrors.InvalidComponentQuantity;
        }

        return new BillOfMaterialComponent(id, billOfMaterialId, itemId, quantityPerUnit);
    }
}
