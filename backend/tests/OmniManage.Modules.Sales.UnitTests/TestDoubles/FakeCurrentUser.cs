using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Sales.UnitTests.TestDoubles;

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;

    public bool IsAuthenticated => true;
}
