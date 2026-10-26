using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.Infrastructure;

public sealed class GuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}
