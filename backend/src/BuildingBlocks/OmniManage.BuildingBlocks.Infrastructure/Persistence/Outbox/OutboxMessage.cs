namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public DateTimeOffset OccurredOnUtc { get; init; }

    public DateTimeOffset? ProcessedOnUtc { get; set; }

    public int Attempts { get; set; }

    public string? Error { get; set; }
}
