using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Domain.Abstractions;

public interface ITenantModuleRepository
{
    Task<TenantModule?> GetAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TenantModule tenantModule,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
