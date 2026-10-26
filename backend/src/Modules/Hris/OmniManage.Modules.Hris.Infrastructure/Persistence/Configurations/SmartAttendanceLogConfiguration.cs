using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Hris.Domain.Attendance;
using OmniManage.Modules.Hris.Domain.Employees;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Configurations;

public sealed class SmartAttendanceLogConfiguration : IEntityTypeConfiguration<SmartAttendanceLog>
{
    public void Configure(EntityTypeBuilder<SmartAttendanceLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("attendance_logs", HrisDbContext.Schema);
        builder.HasKey(log => log.Id).HasName("pk_attendance_logs");
        builder.Property(log => log.Id).ValueGeneratedNever();

        builder.Property(log => log.EmployeeId)
            .IsRequired();

        builder.Property(log => log.ClockInUtc)
            .IsRequired();

        builder.Property(log => log.Latitude)
            .IsRequired()
            .HasPrecision(9, 6);

        builder.Property(log => log.Longitude)
            .IsRequired()
            .HasPrecision(10, 6);

        builder.Property(log => log.PhotoUrl)
            .HasMaxLength(SmartAttendanceLog.PhotoUrlMaxLength);

        builder.Property(log => log.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(log => log.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(log => new { log.TenantId, log.EmployeeId, log.ClockInUtc })
            .HasDatabaseName("ix_attendance_logs_tenant_id_employee_id_clock_in_utc");

        builder.HasIndex(log => new { log.TenantId, log.Status })
            .HasDatabaseName("ix_attendance_logs_tenant_id_status");

        builder.HasIndex(log => new { log.TenantId, log.ClockInUtc })
            .HasDatabaseName("ix_attendance_logs_tenant_id_clock_in_utc");
    }
}
