using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration(string schema) : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("outbox_messages", schema);
        builder.HasKey(message => message.Id).HasName("pk_outbox_messages");

        builder.Property(message => message.Type).IsRequired().HasMaxLength(500);
        builder.Property(message => message.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(message => message.Error).HasMaxLength(4000);

        builder
            .HasIndex(message => new { message.TenantId, message.OccurredOnUtc })
            .HasDatabaseName("ix_outbox_messages_tenant_id_occurred_on_utc");

        builder
            .HasIndex(message => message.OccurredOnUtc)
            .HasDatabaseName("ix_outbox_messages_occurred_on_utc_unprocessed")
            .HasFilter("processed_on_utc IS NULL");
    }
}
