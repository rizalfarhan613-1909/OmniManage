using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FixedGuidGenerator(params Guid[] values) : IGuidGenerator
{
    private readonly Queue<Guid> _values = new(values);

    public Guid NewGuid()
    {
        if (_values.Count == 0)
        {
            throw new InvalidOperationException("No deterministic identifiers remain.");
        }

        return _values.Dequeue();
    }
}
