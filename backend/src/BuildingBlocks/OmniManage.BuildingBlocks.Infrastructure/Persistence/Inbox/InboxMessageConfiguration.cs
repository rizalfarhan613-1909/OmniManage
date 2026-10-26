using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;

public sealed class InboxMessageConfiguration(string schema) : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("inbox_messages", schema);
        builder.HasKey(message => new { message.EventId, message.Consumer }).HasName("pk_inbox_messages");

        builder.Property(message => message.Consumer).IsRequired().HasMaxLength(500);
        builder.Property(message => message.Status).IsRequired().HasMaxLength(40).HasConversion(
            status => ToDatabaseValue(status),
            value => FromDatabaseValue(value));

        builder
            .HasIndex(message => new { message.TenantId, message.EventId })
            .HasDatabaseName("ix_inbox_messages_tenant_id_event_id");
    }

    private static string ToDatabaseValue(InboxStatus status) => status switch
    {
        InboxStatus.Processed => "PROCESSED",
        InboxStatus.SkippedModuleDisabled => "SKIPPED_MODULE_DISABLED",
        InboxStatus.Failed => "FAILED",
        InboxStatus.DeadLetter => "DEAD_LETTER",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported inbox status."),
    };

    private static InboxStatus FromDatabaseValue(string value) => value switch
    {
        "PROCESSED" => InboxStatus.Processed,
        "SKIPPED_MODULE_DISABLED" => InboxStatus.SkippedModuleDisabled,
        "FAILED" => InboxStatus.Failed,
        "DEAD_LETTER" => InboxStatus.DeadLetter,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported inbox status."),
    };
}
