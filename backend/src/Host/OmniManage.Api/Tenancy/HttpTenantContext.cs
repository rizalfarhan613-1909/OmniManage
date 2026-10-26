using System.Security.Claims;
using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Api.Tenancy;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId
    {
        get
        {
            var tenantId = httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");

            return Guid.TryParse(tenantId, out var parsedTenantId) ? parsedTenantId : Guid.Empty;
        }
    }

    // ASSUMPTION: the platform (superadmin) realm is not issued by this host yet.
    public bool IsSuperadminContext => false;
}
