using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OmniManage.Modules.Sales.Application.Orders.ConfirmSalesOrder;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders;
using OmniManage.Modules.Sales.UnitTests.TestDoubles;

namespace OmniManage.Modules.Sales.UnitTests.Application;

public sealed class ConfirmSalesOrderCommandHandlerTests
{
    private static readonly Guid OrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000821");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000822");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000823");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000824");
    private static readonly Guid LineId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000825");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000826");
    private static readonly Guid EventId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000827");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_DraftOrder_ConfirmsPersistsOutboxAndDispatches()
    {
        var order = CreateOrder();
        var repository = new FakeSalesOrderRepository();
        repository.Orders.Add(order);
        var outbox = new FakeIntegrationEventOutbox();
        var dispatcher = new FakeIntegrationEventDispatcher();
        var handler = CreateHandler(repository, outbox, dispatcher);

        var result = await handler.Handle(
            new ConfirmSalesOrderCommand(OrderId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Confirmed");
        result.Value.UpdatedAt.Should().Be(Now);

        order.Status.Should().Be(SalesOrderStatus.Confirmed);
        order.DomainEvents.Should().BeEmpty();
        repository.SaveCount.Should().Be(1);

        outbox.Enqueued.Should().ContainSingle();
        var integrationEvent = outbox.Enqueued[0].Should().BeOfType<SalesOrderConfirmedIntegrationEvent>().Subject;
        integrationEvent.EventId.Should().Be(EventId);
        integrationEvent.TenantId.Should().Be(TenantId);
        integrationEvent.SalesOrderId.Should().Be(OrderId);
        integrationEvent.CustomerId.Should().Be(CustomerId);
        integrationEvent.OccurredOnUtc.Should().Be(Now.UtcDateTime);
        integrationEvent.Lines.Should().ContainSingle();
        integrationEvent.Lines[0].ItemId.Should().Be(ItemId);
        integrationEvent.Lines[0].Quantity.Should().Be(2m);
        integrationEvent.Lines[0].UnitPrice.Should().Be(10.25m);
        integrationEvent.Lines[0].LineTotal.Should().Be(20.5m);

        dispatcher.Dispatched.Should().ContainSingle().Which.Should().BeSameAs(integrationEvent);
        outbox.Processed.Should().ContainSingle().Which.Should().Be(EventId);
        outbox.Failed.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        var repository = new FakeSalesOrderRepository();
        var outbox = new FakeIntegrationEventOutbox();
        var dispatcher = new FakeIntegrationEventDispatcher();
        var handler = CreateHandler(repository, outbox, dispatcher);

        var result = await handler.Handle(
            new ConfirmSalesOrderCommand(OrderId),
            CancellationToken.None);

        result.Error.Should().Be(SalesOrderErrors.NotFound);
        repository.SaveCount.Should().Be(0);
        outbox.Enqueued.Should().BeEmpty();
        dispatcher.Dispatched.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenOrderIsAlreadyConfirmed_ReturnsInvalidStateWithoutEnqueueing()
    {
        var order = CreateOrder();
        order.Confirm(EventId, Now, UserId).IsSuccess.Should().BeTrue();
        order.ClearDomainEvents();

        var repository = new FakeSalesOrderRepository();
        repository.Orders.Add(order);
        var outbox = new FakeIntegrationEventOutbox();
        var dispatcher = new FakeIntegrationEventDispatcher();
        var handler = CreateHandler(repository, outbox, dispatcher);

        var result = await handler.Handle(
            new ConfirmSalesOrderCommand(OrderId),
            CancellationToken.None);

        result.Error.Should().Be(SalesOrderErrors.InvalidState);
        repository.SaveCount.Should().Be(0);
        outbox.Enqueued.Should().BeEmpty();
        dispatcher.Dispatched.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenConsumerFails_KeepsConfirmationAndMarksOutboxFailed()
    {
        var order = CreateOrder();
        var repository = new FakeSalesOrderRepository();
        repository.Orders.Add(order);
        var outbox = new FakeIntegrationEventOutbox();
        var dispatcher = new FakeIntegrationEventDispatcher { Result = false };
        var handler = CreateHandler(repository, outbox, dispatcher);

        var result = await handler.Handle(
            new ConfirmSalesOrderCommand(OrderId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(SalesOrderStatus.Confirmed);
        repository.SaveCount.Should().Be(1);
        outbox.Processed.Should().BeEmpty();
        outbox.Failed.Should().ContainSingle().Which.EventId.Should().Be(EventId);
    }

    [Fact]
    public async Task Handle_WhenDispatcherThrows_KeepsConfirmationAndDoesNotFailTheRequest()
    {
        var order = CreateOrder();
        var repository = new FakeSalesOrderRepository();
        repository.Orders.Add(order);
        var outbox = new FakeIntegrationEventOutbox();
        var dispatcher = new FakeIntegrationEventDispatcher
        {
            Exception = new InvalidOperationException("Broker unavailable."),
        };
        var handler = CreateHandler(repository, outbox, dispatcher);

        var result = await handler.Handle(
            new ConfirmSalesOrderCommand(OrderId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(SalesOrderStatus.Confirmed);
        repository.SaveCount.Should().Be(1);
        outbox.Processed.Should().BeEmpty();
        outbox.Failed.Should().BeEmpty();
    }

    private static ConfirmSalesOrderCommandHandler CreateHandler(
        FakeSalesOrderRepository repository,
        FakeIntegrationEventOutbox outbox,
        FakeIntegrationEventDispatcher dispatcher) =>
        new(
            repository,
            outbox,
            dispatcher,
            new FakeCurrentUser(UserId),
            new FixedGuidGenerator(EventId),
            new FixedTimeProvider(Now),
            NullLogger<ConfirmSalesOrderCommandHandler>.Instance);

    private static SalesOrder CreateOrder() =>
        SalesOrder.Create(
            OrderId,
            TenantId,
            "SO-3001",
            CustomerId,
            [new SalesOrderLineInput(LineId, ItemId, 2m, 10.25m)],
            Now.AddHours(-1),
            UserId).Value;
}
