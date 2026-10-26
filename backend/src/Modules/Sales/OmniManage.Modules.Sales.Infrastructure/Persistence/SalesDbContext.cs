using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Infrastructure.Persistence;

public sealed class SalesDbContext(
    DbContextOptions<SalesDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "sales";

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

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
