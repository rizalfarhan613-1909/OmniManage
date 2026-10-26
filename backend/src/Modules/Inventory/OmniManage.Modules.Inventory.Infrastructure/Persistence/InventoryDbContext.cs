using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.Domain.Stock;
using OmniManage.Modules.Inventory.Domain.Warehouses;

namespace OmniManage.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(
    DbContextOptions<InventoryDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "inventory";

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<StockLedger> StockLedgerEntries => Set<StockLedger>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureLedgerIsAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureLedgerIsAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyOutboxConfiguration(Schema);
        modelBuilder.ApplyInboxConfiguration(Schema);
    }

    private void EnsureLedgerIsAppendOnly()
    {
        var hasForbiddenChanges = ChangeTracker
            .Entries<StockLedger>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted);

        if (hasForbiddenChanges)
        {
            throw new InvalidOperationException(StockErrors.LedgerIsAppendOnly.Message);
        }
    }
}
