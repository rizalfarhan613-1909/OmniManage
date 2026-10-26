using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Hris.Domain.Payroll;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Configurations;

public sealed class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("payroll_runs", HrisDbContext.Schema);
        builder.HasKey(payrollRun => payrollRun.Id).HasName("pk_payroll_runs");
        builder.Property(payrollRun => payrollRun.Id).ValueGeneratedNever();

        builder.Property(payrollRun => payrollRun.PeriodCode)
            .IsRequired()
            .HasMaxLength(PayrollRun.PeriodCodeMaxLength);

        builder.Property(payrollRun => payrollRun.TotalBasicSalary)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(payrollRun => payrollRun.TotalAllowances)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(payrollRun => payrollRun.TotalDeductions)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(payrollRun => payrollRun.NetPayTotal)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(payrollRun => payrollRun.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(payrollRun => new { payrollRun.TenantId, payrollRun.PeriodCode })
            .IsUnique()
            .HasDatabaseName("uq_payroll_runs_tenant_id_period_code");

        builder.HasIndex(payrollRun => new { payrollRun.TenantId, payrollRun.Status })
            .HasDatabaseName("ix_payroll_runs_tenant_id_status");

        builder.HasIndex(payrollRun => new { payrollRun.TenantId, payrollRun.ProcessedAtUtc })
            .HasDatabaseName("ix_payroll_runs_tenant_id_processed_at_utc");
    }
}
