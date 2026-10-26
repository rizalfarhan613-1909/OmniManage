using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class GetSessionQueryHandler(
    AuthSessionBuilder sessionBuilder,
    ITenantContext tenantContext,
    ICurrentUser currentUser)
    : IQueryHandler<GetSessionQuery, Result<AuthSessionDto>>
{
    public Task<Result<AuthSessionDto>> Handle(
        GetSessionQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!currentUser.IsAuthenticated)
        {
            return Task.FromResult<Result<AuthSessionDto>>(AuthErrors.Unauthenticated);
        }

        return sessionBuilder.BuildAsync(tenantContext.TenantId, currentUser.UserId, cancellationToken);
    }
}
