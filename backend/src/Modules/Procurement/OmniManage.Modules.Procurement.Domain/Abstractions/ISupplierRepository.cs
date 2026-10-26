using OmniManage.Modules.Procurement.Domain.Suppliers;

namespace OmniManage.Modules.Procurement.Domain.Abstractions;

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Supplier> Suppliers, long TotalItems)> ListAsync(
        SupplierListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
