using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public const string Token = "test-access-token";

    public IReadOnlyCollection<string> IssuedRoles { get; private set; } = [];

    public Guid IssuedSessionId { get; private set; }

    public AccessToken Generate(
        Guid userId,
        Guid tenantId,
        IReadOnlyCollection<string> roles,
        Guid sessionId)
    {
        IssuedRoles = roles;
        IssuedSessionId = sessionId;

        return new AccessToken(
            Token,
            new DateTimeOffset(2026, 10, 21, 8, 15, 0, TimeSpan.Zero));
    }
}
