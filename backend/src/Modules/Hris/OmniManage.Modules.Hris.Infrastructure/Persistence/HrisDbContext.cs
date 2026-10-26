using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Hris.Domain.Attendance;
using OmniManage.Modules.Hris.Domain.Employees;
using OmniManage.Modules.Hris.Domain.Payroll;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence;

public sealed class HrisDbContext(
    DbContextOptions<HrisDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "hris";

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<SmartAttendanceLog> AttendanceLogs => Set<SmartAttendanceLog>();

    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();

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
