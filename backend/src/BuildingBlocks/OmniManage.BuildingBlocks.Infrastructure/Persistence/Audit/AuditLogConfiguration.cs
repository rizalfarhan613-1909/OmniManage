using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public const string Schema = "audit";

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("audit_logs", Schema);
        builder.HasKey(log => new { log.Id, log.OccurredAt }).HasName("pk_audit_logs");

        builder.Property(log => log.ActorType).IsRequired().HasMaxLength(20);
        builder.Property(log => log.Action).IsRequired().HasMaxLength(100);
        builder.Property(log => log.ModuleCode).HasMaxLength(50);
        builder.Property(log => log.EntityType).HasMaxLength(100);
        builder.Property(log => log.EntityId).HasMaxLength(100);
        builder.Property(log => log.BeforeData).HasColumnType("jsonb");
        builder.Property(log => log.AfterData).HasColumnType("jsonb");
        builder.Property(log => log.IpAddress).HasColumnType("inet");
        builder.Property(log => log.UserAgent).HasColumnType("text");
        builder.Property(log => log.TraceId).HasMaxLength(64);
        builder.Property(log => log.PrevHash).HasColumnType("bytea");
        builder.Property(log => log.RowHash).HasColumnType("bytea");

        builder
            .HasIndex(log => new { log.TenantId, log.OccurredAt })
            .HasDatabaseName("ix_audit_logs_tenant_id_occurred_at");

        builder
            .HasIndex(log => new { log.EntityType, log.EntityId })
            .HasDatabaseName("ix_audit_logs_entity_type_entity_id");

        builder
            .HasIndex(log => new { log.ActorId, log.OccurredAt })
            .HasDatabaseName("ix_audit_logs_actor_id_occurred_at");
    }
}
