using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;

public sealed class AssetMaintenanceDbContext(
    DbContextOptions<AssetMaintenanceDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "asset_maintenance";

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<MaintenanceOrder> MaintenanceOrders => Set<MaintenanceOrder>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyOutboxConfiguration(Schema);
        modelBuilder.ApplyInboxConfiguration(Schema);
    }
}
