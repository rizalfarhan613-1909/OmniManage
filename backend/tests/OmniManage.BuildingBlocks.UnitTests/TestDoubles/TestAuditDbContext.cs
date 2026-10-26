using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

namespace OmniManage.BuildingBlocks.UnitTests.TestDoubles;

public sealed class TestAuditDbContext(DbContextOptions<TestAuditDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<AuditedWidget> Widgets => Set<AuditedWidget>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        modelBuilder.Entity<AuditedWidget>(builder =>
        {
            builder.HasKey(widget => widget.Id);
            builder.Property(widget => widget.Name).IsRequired().HasMaxLength(200);
            builder.Property(widget => widget.SensitiveValue).IsRequired().HasMaxLength(200);
        });
    }
}
