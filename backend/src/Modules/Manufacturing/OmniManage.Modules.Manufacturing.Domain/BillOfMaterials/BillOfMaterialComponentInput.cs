namespace OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

/// <summary>
/// Input used by the aggregate factory. Identifiers are supplied by the
/// application layer from <c>IGuidGenerator</c>; the domain never creates them.
/// </summary>
public sealed record BillOfMaterialComponentInput(
    Guid Id,
    Guid ItemId,
    decimal QuantityPerUnit);
