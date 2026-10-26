namespace OmniManage.Modules.Platform.Application.Auth;

public interface IJwtTokenGenerator
{
    AccessToken Generate(
        Guid userId,
        Guid tenantId,
        IReadOnlyCollection<string> roles,
        Guid sessionId);
}
