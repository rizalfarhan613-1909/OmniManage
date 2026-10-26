using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.Modules.Inventory.Domain.Abstractions;

namespace OmniManage.Modules.Inventory.Infrastructure.Persistence.Inbox;

public sealed class InventoryInbox(InventoryDbContext dbContext) : IInventoryInbox
{
    public Task<bool> IsProcessedAsync(
        Guid eventId,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        return dbContext.InboxMessages.AnyAsync(
            message => message.EventId == eventId
                && message.Consumer == consumer
                && message.Status == InboxStatus.Processed,
            cancellationToken);
    }

    public Task MarkProcessedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default) =>
        UpsertAsync(eventId, consumer, tenantId, InboxStatus.Processed, processedOnUtc, cancellationToken);

    public Task MarkSkippedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default) =>
        UpsertAsync(eventId, consumer, tenantId, InboxStatus.SkippedModuleDisabled, processedOnUtc, cancellationToken);

    public Task MarkFailedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default) =>
        UpsertAsync(eventId, consumer, tenantId, InboxStatus.Failed, processedOnUtc, cancellationToken);

    private async Task UpsertAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        InboxStatus status,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        var existing = await dbContext.InboxMessages
            .FirstOrDefaultAsync(
                message => message.EventId == eventId && message.Consumer == consumer,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            dbContext.InboxMessages.Add(new InboxMessage
            {
                EventId = eventId,
                Consumer = consumer,
                TenantId = tenantId,
                Status = status,
                ProcessedOnUtc = processedOnUtc,
            });
            return;
        }

        existing.Status = status;
        existing.ProcessedOnUtc = processedOnUtc;
    }
}
