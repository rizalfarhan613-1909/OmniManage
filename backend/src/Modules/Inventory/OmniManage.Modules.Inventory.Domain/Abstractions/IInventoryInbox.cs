namespace OmniManage.Modules.Inventory.Domain.Abstractions;

/// <summary>
/// Idempotency store for integration-event consumers in the Inventory module.
/// Implementations stage the inbox row on the module's DbContext so it is
/// committed in the same transaction as the stock-ledger entries.
/// </summary>
public interface IInventoryInbox
{
    Task<bool> IsProcessedAsync(
        Guid eventId,
        string consumer,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default);

    Task MarkSkippedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default);
}
