using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;
using OmniManage.Modules.Procurement.Domain.Suppliers;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence;

public sealed class ProcurementDbContext(
    DbContextOptions<ProcurementDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "procurement";

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
    }
}
