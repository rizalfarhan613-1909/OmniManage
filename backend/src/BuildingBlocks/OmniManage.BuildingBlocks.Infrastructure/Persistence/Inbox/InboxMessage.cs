namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; init; }

    public string Consumer { get; init; } = string.Empty;

    public Guid TenantId { get; init; }

    public InboxStatus Status { get; set; }

    public DateTimeOffset ProcessedOnUtc { get; set; }
}
