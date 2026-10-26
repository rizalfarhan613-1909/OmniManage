using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.MasterData.UnitTests.TestDoubles;

internal sealed class FixedGuidGenerator(params Guid[] values) : IGuidGenerator
{
    private int _index;

    public Guid NewGuid()
    {
        if (_index >= values.Length)
        {
            throw new InvalidOperationException("No deterministic identifiers remain.");
        }

        return values[_index++];
    }
}
