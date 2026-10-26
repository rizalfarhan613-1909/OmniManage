using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyOutboxConfiguration(this ModelBuilder modelBuilder, string schema)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration(schema));

        return modelBuilder;
    }

    public static ModelBuilder ApplyInboxConfiguration(this ModelBuilder modelBuilder, string schema)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration(schema));

        return modelBuilder;
    }
}
