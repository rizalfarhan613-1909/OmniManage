using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Sales.UnitTests.TestDoubles;

internal sealed class FakeIntegrationEventDispatcher : IIntegrationEventDispatcher
{
    public List<IIntegrationEvent> Dispatched { get; } = [];

    public bool Result { get; set; } = true;

    public Exception? Exception { get; set; }

    public Task<bool> DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Dispatched.Add(integrationEvent);

        if (Exception is not null)
        {
            throw Exception;
        }

        return Task.FromResult(Result);
    }
}
