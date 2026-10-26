namespace OmniManage.BuildingBlocks.Domain;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }

    Guid CreatedBy { get; }

    DateTimeOffset? UpdatedAt { get; }

    Guid? UpdatedBy { get; }
}
