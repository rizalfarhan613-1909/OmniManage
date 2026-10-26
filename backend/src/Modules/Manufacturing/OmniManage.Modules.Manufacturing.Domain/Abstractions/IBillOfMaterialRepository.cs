using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

namespace OmniManage.Modules.Manufacturing.Domain.Abstractions;

public interface IBillOfMaterialRepository
{
    Task AddAsync(BillOfMaterial billOfMaterial, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(
        string code,
        int version,
        CancellationToken cancellationToken = default);

    Task<BillOfMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<BillOfMaterial> BillOfMaterials, long TotalItems)> ListAsync(
        BillOfMaterialListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
