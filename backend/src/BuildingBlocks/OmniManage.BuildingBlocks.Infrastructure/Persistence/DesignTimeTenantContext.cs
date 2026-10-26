using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Tenant context used by EF Core design-time factories. Migrations only build
/// the model, so no rows are read and an empty tenant id is safe (RLS fails
/// closed if a query ever runs without a tenant).
/// </summary>
public sealed class DesignTimeTenantContext : ITenantContext
{
    public static DesignTimeTenantContext Instance { get; } = new();

    public Guid TenantId => Guid.Empty;

    public bool IsSuperadminContext => false;
}
