using FluentAssertions;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;
using OmniManage.Modules.Platform.UnitTests.TestDoubles;

namespace OmniManage.Modules.Platform.UnitTests.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000401");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000402");
    private static readonly Guid SessionId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000403");
    private static readonly Guid TokenId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000404");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithActiveToken_RotatesTokenAndReturnsNewSession()
    {
        var fixture = CreateFixture();
        var oldToken = fixture.Repository.Tokens.Single();

        var result = await fixture.Handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshToken.Should().Be("refresh-token-1");
        result.Value.RefreshTokenExpiresAt.Should().Be(Now.AddDays(7));
        result.Value.Session.User.Id.Should().Be(UserId);

        oldToken.RevokedAt.Should().Be(Now);
        oldToken.RevokedReason.Should().Be(RefreshTokenRevokedReasons.Rotated);
        oldToken.ReplacedBy.Should().NotBeNull();

        fixture.Repository.Tokens.Should().HaveCount(2);
        var replacement = fixture.Repository.Tokens.Single(token => token.Id == oldToken.ReplacedBy);
        replacement.TokenHash.Should().Be(fixture.Generator.Hash("refresh-token-1"));
        replacement.SessionId.Should().Be(SessionId);
        replacement.IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRotatedTokenIsReused_RevokesEntireSession()
    {
        var fixture = CreateFixture();

        var first = await fixture.Handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var reused = await fixture.Handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);

        reused.IsFailure.Should().BeTrue();
        reused.Error.Should().Be(RefreshTokenErrors.InvalidRefreshToken);
        fixture.Repository.Tokens.Should().OnlyContain(token => token.RevokedAt != null);
        fixture.Repository.Tokens.Should().OnlyContain(
            token => token.RevokedReason == RefreshTokenRevokedReasons.ReuseDetected
                || token.RevokedReason == RefreshTokenRevokedReasons.Rotated);
        fixture.AuditLogger.Entries.Should().ContainSingle()
            .Which.Action.Should().Be("auth.refresh_token.reuse");
        fixture.AuditLogger.FailClosedFlags.Should().ContainSingle().Which.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsInvalidAndRevokesToken()
    {
        var fixture = CreateFixture(issuedAt: Now.AddDays(-10), expiresAt: Now.AddDays(-1));

        var result = await fixture.Handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidRefreshToken);
        fixture.Repository.Tokens.Single().RevokedReason.Should().Be(RefreshTokenRevokedReasons.Expired);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsInvalidWithoutMutation()
    {
        var fixture = CreateFixture();

        var result = await fixture.Handler.Handle(
            new RefreshTokenCommand("unknown-token"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidRefreshToken);
        fixture.Repository.Tokens.Should().ContainSingle().Which.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Logout_RevokesSession()
    {
        var fixture = CreateFixture();

        var result = await fixture.LogoutHandler.Handle(
            new LogoutCommand("old-token"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Revoked.Should().BeTrue();
        fixture.Repository.Tokens.Single().RevokedReason.Should().Be(RefreshTokenRevokedReasons.Logout);
        fixture.AuditLogger.Entries.Should().ContainSingle()
            .Which.Action.Should().Be("auth.logout");
    }

    private static Fixture CreateFixture(
        DateTimeOffset? issuedAt = null,
        DateTimeOffset? expiresAt = null)
    {
        var user = User.Create(UserId, TenantId, "admin@acme.test", "hashed-password").Value;
        var users = new FakeUserRepository
        {
            UserById = user,
            Roles = [SystemRoles.TenantAdmin],
            PermissionCodes = [Permissions.Sales.OrderRead],
        };
        var tenants = new FakeTenantRepository
        {
            Tenant = Tenant.Create(TenantId, "ACME", "Acme Corporation", "STARTER", Now).Value,
        };
        var licenses = new FakeTenantModuleCache
        {
            Snapshot = new LicenseSnapshot([ModuleCodes.SalesCore]),
        };
        var generator = new FakeRefreshTokenGenerator();
        var repository = new FakeRefreshTokenRepository();
        var auditLogger = new FakeAuditLogger();
        var clock = new FixedTimeProvider(Now);
        var token = RefreshToken.Issue(
            TokenId,
            TenantId,
            UserId,
            SessionId,
            generator.Hash("old-token"),
            "device-1",
            "203.0.113.10",
            "xunit",
            issuedAt ?? Now.AddHours(-1),
            expiresAt ?? Now.AddDays(7)).Value;

        repository.Tokens.Add(token);

        var sessionBuilder = new AuthSessionBuilder(users, tenants, licenses);
        var sessionRevoker = new RefreshTokenSessionRevoker(repository, clock);
        var options = new RefreshTokenOptions();

        var handler = new RefreshTokenCommandHandler(
            repository,
            sessionBuilder,
            new FakeJwtTokenGenerator(),
            generator,
            new FakeGuidGenerator(),
            options,
            sessionRevoker,
            clock,
            auditLogger);

        var logoutHandler = new LogoutCommandHandler(
            repository,
            generator,
            sessionRevoker,
            auditLogger);

        return new Fixture(handler, logoutHandler, repository, generator, auditLogger);
    }

    private sealed record Fixture(
        RefreshTokenCommandHandler Handler,
        LogoutCommandHandler LogoutHandler,
        FakeRefreshTokenRepository Repository,
        FakeRefreshTokenGenerator Generator,
        FakeAuditLogger AuditLogger);
}
