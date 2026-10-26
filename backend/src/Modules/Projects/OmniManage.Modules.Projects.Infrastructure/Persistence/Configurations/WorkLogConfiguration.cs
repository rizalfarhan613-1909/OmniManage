using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Projects.Domain.Tasks;
using OmniManage.Modules.Projects.Domain.WorkLogs;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Configurations;

public sealed class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
{
    public void Configure(EntityTypeBuilder<WorkLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("work_logs", ProjectsDbContext.Schema);
        builder.HasKey(workLog => workLog.Id).HasName("pk_work_logs");
        builder.Property(workLog => workLog.Id).ValueGeneratedNever();

        builder.Property(workLog => workLog.ProjectTaskId)
            .IsRequired();

        builder.Property(workLog => workLog.EmployeeId)
            .IsRequired();

        builder.Property(workLog => workLog.LoggedDate)
            .IsRequired();

        builder.Property(workLog => workLog.HoursSpent)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(workLog => workLog.Description)
            .IsRequired()
            .HasMaxLength(WorkLog.DescriptionMaxLength);

        builder.HasOne<ProjectTask>()
            .WithMany()
            .HasForeignKey(workLog => workLog.ProjectTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(workLog => new { workLog.TenantId, workLog.ProjectTaskId, workLog.LoggedDate })
            .HasDatabaseName("ix_work_logs_tenant_id_project_task_id_logged_date");

        builder.HasIndex(workLog => new { workLog.TenantId, workLog.EmployeeId, workLog.LoggedDate })
            .HasDatabaseName("ix_work_logs_tenant_id_employee_id_logged_date");
    }
}
