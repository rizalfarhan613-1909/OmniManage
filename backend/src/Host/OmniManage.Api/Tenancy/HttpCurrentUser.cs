using System.Security.Claims;
using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Api.Tenancy;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
