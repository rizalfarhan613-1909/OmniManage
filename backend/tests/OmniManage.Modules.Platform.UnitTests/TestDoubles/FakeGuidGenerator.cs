using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeGuidGenerator : IGuidGenerator
{
    private int _sequence;

    public Guid NewGuid() =>
        Guid.Parse($"0192f5b4-8a00-7000-8000-{++_sequence:D12}");
}
