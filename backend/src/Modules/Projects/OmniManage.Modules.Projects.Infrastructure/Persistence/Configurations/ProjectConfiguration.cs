using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Projects.Domain.Projects;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("projects", ProjectsDbContext.Schema);
        builder.HasKey(project => project.Id).HasName("pk_projects");
        builder.Property(project => project.Id).ValueGeneratedNever();

        builder.Property(project => project.Code)
            .IsRequired()
            .HasMaxLength(Project.CodeMaxLength);

        builder.Property(project => project.Name)
            .IsRequired()
            .HasMaxLength(Project.NameMaxLength);

        builder.Property(project => project.Description)
            .HasMaxLength(Project.DescriptionMaxLength);

        builder.Property(project => project.CustomerId)
            .IsRequired();

        builder.Property(project => project.StartDate)
            .IsRequired();

        builder.Property(project => project.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(project => new { project.TenantId, project.Code })
            .IsUnique()
            .HasDatabaseName("uq_projects_tenant_id_code");

        builder.HasIndex(project => new { project.TenantId, project.Name })
            .HasDatabaseName("ix_projects_tenant_id_name");

        builder.HasIndex(project => new { project.TenantId, project.CustomerId })
            .HasDatabaseName("ix_projects_tenant_id_customer_id");

        builder.HasIndex(project => new { project.TenantId, project.Status })
            .HasDatabaseName("ix_projects_tenant_id_status");

        builder.HasIndex(project => new { project.TenantId, project.StartDate })
            .HasDatabaseName("ix_projects_tenant_id_start_date");
    }
}
