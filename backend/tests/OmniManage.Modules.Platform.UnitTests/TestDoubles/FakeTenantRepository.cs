using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeTenantRepository : ITenantRepository
{
    public Tenant? Tenant { get; set; }

    public Task<Tenant?> GetByCodeAsync(
        string normalizedCode,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Tenant is not null && Tenant.Code == normalizedCode ? Tenant : null);

    public Task<Tenant?> GetByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Tenant is not null && Tenant.Id == tenantId ? Tenant : null);
}
