using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Hris.Domain.Employees;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("employees", HrisDbContext.Schema);
        builder.HasKey(employee => employee.Id).HasName("pk_employees");
        builder.Property(employee => employee.Id).ValueGeneratedNever();

        builder.Property(employee => employee.EmployeeCode)
            .IsRequired()
            .HasMaxLength(Employee.EmployeeCodeMaxLength);

        builder.Property(employee => employee.FullName)
            .IsRequired()
            .HasMaxLength(Employee.FullNameMaxLength);

        builder.Property(employee => employee.Email)
            .IsRequired()
            .HasMaxLength(Employee.EmailMaxLength);

        builder.Property(employee => employee.JobTitle)
            .IsRequired()
            .HasMaxLength(Employee.JobTitleMaxLength);

        builder.Property(employee => employee.DepartmentId)
            .IsRequired();

        builder.Property(employee => employee.JoinedDate)
            .IsRequired();

        builder.Property(employee => employee.IsActive)
            .IsRequired();

        builder.HasIndex(employee => new { employee.TenantId, employee.EmployeeCode })
            .IsUnique()
            .HasDatabaseName("uq_employees_tenant_id_employee_code");

        builder.HasIndex(employee => new { employee.TenantId, employee.Email })
            .IsUnique()
            .HasDatabaseName("uq_employees_tenant_id_email");

        builder.HasIndex(employee => new { employee.TenantId, employee.DepartmentId })
            .HasDatabaseName("ix_employees_tenant_id_department_id");

        builder.HasIndex(employee => new { employee.TenantId, employee.IsActive })
            .HasDatabaseName("ix_employees_tenant_id_is_active");
    }
}
