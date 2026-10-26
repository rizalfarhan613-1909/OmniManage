using System.Diagnostics;
using System.Security.Claims;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;

namespace OmniManage.Api.Tenancy;

public sealed class HttpAuditContext(
    IHttpContextAccessor httpContextAccessor,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : IAuditContext
{
    public Guid? TenantId => tenantContext.TenantId == Guid.Empty ? null : tenantContext.TenantId;

    public Guid? ActorId => currentUser.UserId == Guid.Empty ? null : currentUser.UserId;

    public string ActorType => currentUser.IsAuthenticated ? "User" : "System";

    public Guid? ImpersonatedBy
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("impersonated_by");

            return Guid.TryParse(value, out var impersonatedBy) && impersonatedBy != Guid.Empty
                ? impersonatedBy
                : null;
        }
    }

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent
    {
        get
        {
            var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

            return string.IsNullOrWhiteSpace(userAgent) ? null : userAgent;
        }
    }

    public string? TraceId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;

            return httpContext is null
                ? Activity.Current?.Id
                : Activity.Current?.Id ?? httpContext.TraceIdentifier;
        }
    }
}
