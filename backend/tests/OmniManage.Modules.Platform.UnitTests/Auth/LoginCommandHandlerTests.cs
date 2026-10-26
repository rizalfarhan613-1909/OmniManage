using FluentAssertions;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;
using OmniManage.Modules.Platform.Infrastructure.Auth;
using OmniManage.Modules.Platform.UnitTests.TestDoubles;

namespace OmniManage.Modules.Platform.UnitTests.Auth;

public sealed class LoginCommandHandlerTests
{
    private const string Password = "CorrectHorseBatteryStaple!";

    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000301");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000302");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokenAndSession()
    {
        var passwordHasher = new Argon2PasswordHasher();
        var user = CreateUser(passwordHasher.HashPassword(Password));
        var users = new FakeUserRepository
        {
            UserByEmail = user,
            UserById = user,
            Roles = [SystemRoles.TenantAdmin],
            PermissionCodes = [Permissions.Sales.OrderRead],
        };
        var tenants = new FakeTenantRepository { Tenant = CreateTenant() };
        var licenses = new FakeTenantModuleCache
        {
            Snapshot = new LicenseSnapshot([ModuleCodes.SalesCore]),
        };
        var tokens = new FakeJwtTokenGenerator();
        var handler = CreateHandler(passwordHasher, users, tenants, licenses, tokens);

        var result = await handler.Handle(
            new LoginCommand("acme", " ADMIN@ACME.TEST ", Password),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(FakeJwtTokenGenerator.Token);
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshTokenExpiresAt.Should().Be(Now.AddDays(7));
        result.Value.Session.User.Id.Should().Be(UserId);
        result.Value.Session.Tenant.Code.Should().Be("ACME");
        result.Value.Session.ActiveModules.Should().ContainSingle()
            .Which.Should().Be(ModuleCodes.SalesCore);
        result.Value.Session.Permissions.Should().ContainSingle()
            .Which.Should().Be(Permissions.Sales.OrderRead);
        result.Value.Session.User.Roles.Should().ContainSingle()
            .Which.Should().Be(SystemRoles.TenantAdmin);
        tokens.IssuedRoles.Should().Equal(SystemRoles.TenantAdmin);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsInvalidCredentials()
    {
        var passwordHasher = new Argon2PasswordHasher();
        var user = CreateUser(passwordHasher.HashPassword(Password));
        var handler = CreateHandler(
            passwordHasher,
            new FakeUserRepository { UserByEmail = user, UserById = user },
            new FakeTenantRepository { Tenant = CreateTenant() },
            new FakeTenantModuleCache(),
            new FakeJwtTokenGenerator());

        var result = await handler.Handle(
            new LoginCommand("ACME", "admin@acme.test", "WrongPassword123!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WhenTenantDoesNotExist_ReturnsInvalidCredentials()
    {
        var passwordHasher = new Argon2PasswordHasher();
        var handler = CreateHandler(
            passwordHasher,
            new FakeUserRepository(),
            new FakeTenantRepository(),
            new FakeTenantModuleCache(),
            new FakeJwtTokenGenerator());

        var result = await handler.Handle(
            new LoginCommand("UNKNOWN", "admin@acme.test", Password),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ReturnsInvalidCredentials()
    {
        var passwordHasher = new Argon2PasswordHasher();
        var user = CreateUser(passwordHasher.HashPassword(Password));
        user.Deactivate();

        var handler = CreateHandler(
            passwordHasher,
            new FakeUserRepository { UserByEmail = user, UserById = user },
            new FakeTenantRepository { Tenant = CreateTenant() },
            new FakeTenantModuleCache(),
            new FakeJwtTokenGenerator());

        var result = await handler.Handle(
            new LoginCommand("ACME", "admin@acme.test", Password),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WhenPermissionModuleIsInactive_ExcludesPermission()
    {
        var passwordHasher = new Argon2PasswordHasher();
        var user = CreateUser(passwordHasher.HashPassword(Password));
        var users = new FakeUserRepository
        {
            UserByEmail = user,
            UserById = user,
            PermissionCodes = [Permissions.Sales.OrderRead],
        };
        var handler = CreateHandler(
            passwordHasher,
            users,
            new FakeTenantRepository { Tenant = CreateTenant() },
            new FakeTenantModuleCache { Snapshot = LicenseSnapshot.Empty },
            new FakeJwtTokenGenerator());

        var result = await handler.Handle(
            new LoginCommand("ACME", "admin@acme.test", Password),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Session.ActiveModules.Should().BeEmpty();
        result.Value.Session.Permissions.Should().BeEmpty();
    }

    private static LoginCommandHandler CreateHandler(
        IPasswordHasher passwordHasher,
        FakeUserRepository users,
        FakeTenantRepository tenants,
        FakeTenantModuleCache licenses,
        FakeJwtTokenGenerator tokens) =>
        new(
            users,
            tenants,
            passwordHasher,
            new AuthSessionBuilder(users, tenants, licenses),
            tokens,
            new FakeRefreshTokenRepository(),
            new FakeRefreshTokenGenerator(),
            new FakeGuidGenerator(),
            new RefreshTokenOptions(),
            new FixedTimeProvider(Now));

    private static User CreateUser(string passwordHash) =>
        User.Create(UserId, TenantId, "admin@acme.test", passwordHash).Value;

    private static Tenant CreateTenant() =>
        Tenant.Create(TenantId, "ACME", "Acme Corporation", "STARTER", Now).Value;
}
