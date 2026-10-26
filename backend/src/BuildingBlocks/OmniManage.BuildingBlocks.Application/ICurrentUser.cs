namespace OmniManage.BuildingBlocks.Application;

public interface ICurrentUser
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}
