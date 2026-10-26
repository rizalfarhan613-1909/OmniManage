using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Sales.UnitTests.TestDoubles;

internal sealed class FixedGuidGenerator(params Guid[] values) : IGuidGenerator
{
    private int _index;

    public IReadOnlyList<Guid> GeneratedValues => values;

    public Guid NewGuid()
    {
        if (_index >= values.Length)
        {
            throw new InvalidOperationException("No deterministic identifiers remain.");
        }

        return values[_index++];
    }
}
