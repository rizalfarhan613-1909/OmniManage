using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence;

public sealed class ManufacturingDbContext(
    DbContextOptions<ManufacturingDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "manufacturing";

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<BillOfMaterial> BillOfMaterials => Set<BillOfMaterial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
    }
}
