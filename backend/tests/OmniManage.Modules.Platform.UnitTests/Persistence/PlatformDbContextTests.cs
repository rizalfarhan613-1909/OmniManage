using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;
using OmniManage.Modules.Platform.Infrastructure.Persistence;

namespace OmniManage.Modules.Platform.UnitTests.Persistence;

public sealed class PlatformDbContextTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000201");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000202");
    private static readonly Guid RoleId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000203");
    private static readonly Guid ChangedBy = Guid.Parse("0192f5b4-8a00-7000-8000-000000000204");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Model_MapsPlatformEntitiesToPlatformSchema()
    {
        using var context = CreateContext();

        AssertTable<Tenant>(context, "tenants");
        AssertTable<TenantModule>(context, "tenant_modules");
        AssertTable<User>(context, "users");
        AssertTable<Role>(context, "roles");
        AssertTable<Permission>(context, "permissions");
        AssertTable<UserRole>(context, "user_roles");
        AssertTable<RolePermission>(context, "role_permissions");
        AssertTable<RefreshToken>(context, "refresh_tokens");
    }

    [Fact]
    public void Model_MapsAuditLogToAuditSchema()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(AuditLog));

        entityType.Should().NotBeNull();
        entityType!.GetSchema().Should().Be(AuditLogConfiguration.Schema);
        entityType!.GetTableName().Should().Be("audit_logs");
        entityType!.FindPrimaryKey()!.Properties.Select(property => property.Name)
            .Should().Equal(nameof(AuditLog.Id), nameof(AuditLog.OccurredAt));
    }

    [Fact]
    public void Model_UsesCompositeKeyForTenantModule()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(TenantModule));
        entityType.Should().NotBeNull();

        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Select(property => property.Name)
            .Should().Equal(nameof(TenantModule.TenantId), nameof(TenantModule.ModuleCode));
    }

    [Fact]
    public void Model_ConfiguresTenantModuleConcurrencyAndActiveIndex()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(TenantModule));
        entityType.Should().NotBeNull();

        entityType!.FindProperty(nameof(TenantModule.Version))!.IsConcurrencyToken.Should().BeTrue();

        var activeIndex = entityType.GetIndexes()
            .Single(index => index.GetDatabaseName() == "ix_tenant_modules_tenant_id_active");
        activeIndex.GetFilter().Should().Be("is_active");
    }

    [Fact]
    public void Model_DoesNotApplyTenantQueryFilterOnPlatformSchema()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(TenantModule));

        entityType!.GetQueryFilter().Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_PersistsTenantLicenseAndRbacMappings()
    {
        var tenant = Tenant.Create(TenantId, "ACME", "Acme Corporation", "STARTER", Now).Value;
        var user = User.Create(UserId, TenantId, "admin@acme.test", "hashed-password").Value;
        var role = Role.Create(RoleId, TenantId, SystemRoles.TenantAdmin, isSystem: true).Value;
        var permission = Permission.Create("finance.invoice.approve").Value;

        user.AssignRole(role).IsSuccess.Should().BeTrue();
        role.GrantPermission(permission).IsSuccess.Should().BeTrue();

        var tenantModule = TenantModule
            .CreateActive(TenantId, "FIN_AR", ChangedBy, "Initial subscription", Now, Now.AddYears(1))
            .Value;

        var databaseName = $"omnimanage-platform-{Guid.NewGuid():N}";

        await using (var context = CreateContext(databaseName))
        {
            context.Tenants.Add(tenant);
            context.Users.Add(user);
            context.Roles.Add(role);
            context.Permissions.Add(permission);
            context.TenantModules.Add(tenantModule);

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext(databaseName))
        {
            var persistedUser = await context.Users
                .Include(item => item.UserRoles)
                .SingleAsync();

            persistedUser.UserRoles.Should().ContainSingle()
                .Which.RoleId.Should().Be(RoleId);

            var persistedRole = await context.Roles
                .Include(item => item.RolePermissions)
                .SingleAsync();

            persistedRole.RolePermissions.Should().ContainSingle()
                .Which.PermissionId.Should().Be(permission.Id);

            var persistedLicense = await context.TenantModules.SingleAsync();
            persistedLicense.ModuleCode.Should().Be("FIN_AR");
            persistedLicense.IsEffectiveAt(Now).Should().BeTrue();
        }
    }

    private static void AssertTable<TEntity>(PlatformDbContext context, string expectedTableName)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        entityType.Should().NotBeNull();
        entityType!.GetSchema().Should().Be(PlatformDbContext.Schema);
        entityType!.GetTableName().Should().Be(expectedTableName);
    }

    private static PlatformDbContext CreateContext() =>
        CreateContext($"omnimanage-platform-{Guid.NewGuid():N}");

    private static PlatformDbContext CreateContext(string databaseName) =>
        new(new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);
}
