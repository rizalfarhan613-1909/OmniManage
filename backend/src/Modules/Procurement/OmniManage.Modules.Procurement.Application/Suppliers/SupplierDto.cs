using OmniManage.Modules.Procurement.Domain.Suppliers;

namespace OmniManage.Modules.Procurement.Application.Suppliers;

public sealed record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    int PaymentTermsDays,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static SupplierDto From(Supplier supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);

        return new SupplierDto(
            supplier.Id,
            supplier.Code,
            supplier.Name,
            supplier.ContactName,
            supplier.Email,
            supplier.Phone,
            supplier.PaymentTermsDays,
            supplier.IsActive,
            supplier.CreatedAt,
            supplier.UpdatedAt);
    }
}
