using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Projects.Domain.Projects;
using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Configurations;

public sealed class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tasks", ProjectsDbContext.Schema);
        builder.HasKey(task => task.Id).HasName("pk_tasks");
        builder.Property(task => task.Id).ValueGeneratedNever();

        builder.Property(task => task.ProjectId)
            .IsRequired();

        builder.Property(task => task.Title)
            .IsRequired()
            .HasMaxLength(ProjectTask.TitleMaxLength);

        builder.Property(task => task.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(task => task.EstimatedHours)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(task => task.CompletedHours)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(task => new { task.TenantId, task.ProjectId })
            .HasDatabaseName("ix_tasks_tenant_id_project_id");

        builder.HasIndex(task => new { task.TenantId, task.AssigneeEmployeeId })
            .HasDatabaseName("ix_tasks_tenant_id_assignee_employee_id");

        builder.HasIndex(task => new { task.TenantId, task.Status })
            .HasDatabaseName("ix_tasks_tenant_id_status");
    }
}
