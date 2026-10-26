namespace OmniManage.BuildingBlocks.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}
