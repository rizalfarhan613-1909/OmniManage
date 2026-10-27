using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Repositories;

public sealed class TenantModuleRepository(PlatformDbContext dbContext) : ITenantModuleRepository
{
    public Task<TenantModule?> GetAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);

        var normalizedCode = moduleCode.Trim().ToUpperInvariant();

        return dbContext.TenantModules.FirstOrDefaultAsync(
            tenantModule => tenantModule.TenantId == tenantId
                && tenantModule.ModuleCode == normalizedCode,
            cancellationToken);
    }

    public Task AddAsync(
        TenantModule tenantModule,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantModule);

        return dbContext.TenantModules.AddAsync(tenantModule, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
