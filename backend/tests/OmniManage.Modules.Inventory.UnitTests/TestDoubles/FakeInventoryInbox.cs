using OmniManage.Modules.Inventory.Domain.Abstractions;

namespace OmniManage.Modules.Inventory.UnitTests.TestDoubles;

internal sealed class FakeInventoryInbox : IInventoryInbox
{
    public List<(Guid EventId, string Consumer, Guid TenantId, string Status)> Entries { get; } = [];

    public bool AlreadyProcessed { get; set; }

    public Task<bool> IsProcessedAsync(
        Guid eventId,
        string consumer,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(AlreadyProcessed);

    public Task MarkProcessedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default)
    {
        Entries.Add((eventId, consumer, tenantId, "Processed"));
        return Task.CompletedTask;
    }

    public Task MarkSkippedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default)
    {
        Entries.Add((eventId, consumer, tenantId, "SkippedModuleDisabled"));
        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default)
    {
        Entries.Add((eventId, consumer, tenantId, "Failed"));
        return Task.CompletedTask;
    }
}
