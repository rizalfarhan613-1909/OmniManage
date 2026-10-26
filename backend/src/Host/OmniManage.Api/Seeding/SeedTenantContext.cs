using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Mutable tenant context used while seeding. The runtime context comes from
/// the JWT; a startup seeder has no HTTP request, so it supplies the demo
/// tenant explicitly. The value is set before any DbContext is created so the
/// RLS connection interceptor applies it on connection open.
/// </summary>
internal sealed class SeedTenantContext(Guid tenantId) : ITenantContext
{
    public Guid TenantId { get; } = tenantId;

    public bool IsSuperadminContext => false;
}
