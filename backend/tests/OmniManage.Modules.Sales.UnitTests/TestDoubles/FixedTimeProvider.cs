namespace OmniManage.Modules.Sales.UnitTests.TestDoubles;

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
