using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;

namespace OmniManage.BuildingBlocks.UnitTests.TestDoubles;

public sealed class TestTenantDbContext(
    DbContextOptions<TestTenantDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public DbSet<TenantWidget> Widgets => Set<TenantWidget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TenantWidget>(builder =>
        {
            builder.HasKey(widget => widget.Id);
            builder.Property(widget => widget.Name).IsRequired().HasMaxLength(200);
        });
    }
}
