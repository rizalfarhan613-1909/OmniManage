using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Finance.Application.IntegrationEvents;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FakeFinanceOutbox : IFinanceOutbox
{
    public List<IIntegrationEvent> Events { get; } = [];

    public Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Events.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
