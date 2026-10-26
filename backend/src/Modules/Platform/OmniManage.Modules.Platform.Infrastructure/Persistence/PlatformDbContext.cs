using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence;

public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : BaseDbContext(options)
{
    public const string Schema = "platform";

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();

    public DbSet<TenantModule> TenantModules => Set<TenantModule>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }
}
