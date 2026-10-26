using System.Globalization;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

namespace OmniManage.Modules.Manufacturing.Application.BillOfMaterials;

public sealed record BillOfMaterialDto(
    Guid Id,
    string Code,
    string Name,
    Guid ProductItemId,
    int Version,
    bool IsActive,
    string? Notes,
    IReadOnlyList<BillOfMaterialComponentDto> Components,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static BillOfMaterialDto From(BillOfMaterial billOfMaterial)
    {
        ArgumentNullException.ThrowIfNull(billOfMaterial);

        return new BillOfMaterialDto(
            billOfMaterial.Id,
            billOfMaterial.Code,
            billOfMaterial.Name,
            billOfMaterial.ProductItemId,
            billOfMaterial.Version,
            billOfMaterial.IsActive,
            billOfMaterial.Notes,
            billOfMaterial.Components.Select(BillOfMaterialComponentDto.From).ToArray(),
            billOfMaterial.CreatedAt,
            billOfMaterial.UpdatedAt);
    }
}

public sealed record BillOfMaterialComponentDto(
    Guid Id,
    Guid ItemId,
    string QuantityPerUnit)
{
    internal static BillOfMaterialComponentDto From(BillOfMaterialComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        return new BillOfMaterialComponentDto(
            component.Id,
            component.ItemId,
            component.QuantityPerUnit.ToString("0.0000", CultureInfo.InvariantCulture));
    }
}
