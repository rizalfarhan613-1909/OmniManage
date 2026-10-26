using FluentAssertions;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders;
using OmniManage.Modules.Sales.Domain.Orders.Events;

namespace OmniManage.Modules.Sales.UnitTests.Domain;

public sealed class SalesOrderTests
{
    private static readonly Guid OrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000801");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000802");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000803");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000804");
    private static readonly Guid LineId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000805");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000806");
    private static readonly Guid EventId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000807");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_NormalizesOrderNumberAndCalculatesTotal()
    {
        var result = Create(lines: [new SalesOrderLineInput(LineId, ItemId, 1.5m, 10.3333m)]);

        result.IsSuccess.Should().BeTrue();

        var order = result.Value;
        order.Id.Should().Be(OrderId);
        order.TenantId.Should().Be(TenantId);
        order.OrderNumber.Should().Be("SO-1001");
        order.CustomerId.Should().Be(CustomerId);
        order.Status.Should().Be(SalesOrderStatus.Draft);
        order.TotalAmount.Should().Be(15.5000m);
        order.CreatedBy.Should().Be(UserId);
        order.Lines.Should().ContainSingle();
        order.Lines[0].LineTotal.Should().Be(15.5000m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-SO-1")]
    [InlineData("SO 1")]
    public void Create_WhenOrderNumberIsInvalid_ReturnsInvalidOrderNumber(string? orderNumber)
    {
        var result = Create(orderNumber: orderNumber);

        result.Error.Should().Be(SalesOrderErrors.InvalidOrderNumber);
    }

    [Fact]
    public void Create_WhenOrderNumberExceedsMaximumLength_ReturnsInvalidOrderNumber()
    {
        var result = Create(orderNumber: new string('S', SalesOrder.OrderNumberMaxLength + 1));

        result.Error.Should().Be(SalesOrderErrors.InvalidOrderNumber);
    }

    [Fact]
    public void Create_WithoutLines_ReturnsNoLines()
    {
        var result = Create(lines: []);

        result.Error.Should().Be(SalesOrderErrors.NoLines);
    }

    [Fact]
    public void Create_WhenCustomerIsEmpty_ReturnsInvalidCustomerId()
    {
        var result = Create(customerId: Guid.Empty);

        result.Error.Should().Be(SalesOrderErrors.InvalidCustomerId);
    }

    [Fact]
    public void Create_WhenLineQuantityIsZero_ReturnsInvalidQuantity()
    {
        var result = Create(lines: [new SalesOrderLineInput(LineId, ItemId, 0m, 1m)]);

        result.Error.Should().Be(SalesOrderErrors.InvalidQuantity);
    }

    [Fact]
    public void Create_WhenLineUnitPriceIsNegative_ReturnsInvalidUnitPrice()
    {
        var result = Create(lines: [new SalesOrderLineInput(LineId, ItemId, 1m, -1m)]);

        result.Error.Should().Be(SalesOrderErrors.InvalidUnitPrice);
    }

    [Fact]
    public void Create_WhenLineItemIsEmpty_ReturnsInvalidItemId()
    {
        var result = Create(lines: [new SalesOrderLineInput(LineId, Guid.Empty, 1m, 1m)]);

        result.Error.Should().Be(SalesOrderErrors.InvalidItemId);
    }

    [Fact]
    public void Create_AccumulatesMultipleLineTotals()
    {
        var secondLineId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000808");

        var result = Create(
            lines:
            [
                new SalesOrderLineInput(LineId, ItemId, 2m, 10.5m),
                new SalesOrderLineInput(secondLineId, ItemId, 3m, 4.25m),
            ]);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalAmount.Should().Be(33.7500m);
    }

    [Fact]
    public void Confirm_DraftOrder_ChangesStatusAndRaisesDomainEvent()
    {
        var order = Create().Value;

        var result = order.Confirm(EventId, Now, UserId);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(SalesOrderStatus.Confirmed);
        order.UpdatedAt.Should().Be(Now);
        order.UpdatedBy.Should().Be(UserId);

        var domainEvent = order.DomainEvents.OfType<SalesOrderConfirmedDomainEvent>().Should().ContainSingle().Subject;
        domainEvent.EventId.Should().Be(EventId);
        domainEvent.SalesOrderId.Should().Be(OrderId);
        domainEvent.TenantId.Should().Be(TenantId);
        domainEvent.OccurredOnUtc.Should().Be(Now);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ReturnsInvalidStateAndDoesNotRaiseAnotherEvent()
    {
        var order = Create().Value;
        order.Confirm(EventId, Now, UserId).IsSuccess.Should().BeTrue();
        order.ClearDomainEvents();

        var result = order.Confirm(Guid.NewGuid(), Now, UserId);

        result.Error.Should().Be(SalesOrderErrors.InvalidState);
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Confirm_WhenEventIdIsEmpty_ReturnsInvalidEventId()
    {
        var result = Create().Value.Confirm(Guid.Empty, Now, UserId);

        result.Error.Should().Be(SalesOrderErrors.InvalidEventId);
    }

    [Fact]
    public void Confirm_WhenOccurredAtIsMissing_ReturnsInvalidUpdatedAt()
    {
        var result = Create().Value.Confirm(EventId, default, UserId);

        result.Error.Should().Be(SalesOrderErrors.InvalidUpdatedAt);
    }

    [Fact]
    public void Confirm_WhenUpdatedByIsEmpty_ReturnsInvalidUpdatedBy()
    {
        var result = Create().Value.Confirm(EventId, Now, Guid.Empty);

        result.Error.Should().Be(SalesOrderErrors.InvalidUpdatedBy);
    }

    private static OmniManage.BuildingBlocks.Domain.Result<SalesOrder> Create(
        string? orderNumber = " so-1001 ",
        Guid? customerId = null,
        IReadOnlyList<SalesOrderLineInput>? lines = null) =>
        SalesOrder.Create(
            OrderId,
            TenantId,
            orderNumber,
            customerId ?? CustomerId,
            lines ?? [new SalesOrderLineInput(LineId, ItemId, 1m, 10m)],
            Now,
            UserId);
}
