using FluentAssertions;
using OmniManage.Modules.Sales.Application.Orders.GetSalesOrderById;
using OmniManage.Modules.Sales.Application.Orders.ListSalesOrders;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders;
using OmniManage.Modules.Sales.UnitTests.TestDoubles;

namespace OmniManage.Modules.Sales.UnitTests.Application;

public sealed class SalesOrderQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000831");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000832");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000833");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task List_WithStatusSearchAndSort_NormalizesCriteriaAndReturnsPage()
    {
        var repository = new FakeSalesOrderRepository();
        var order = CreateOrder(1, Now);
        order.Confirm(Guid.NewGuid(), Now, UserId).IsSuccess.Should().BeTrue();
        repository.Orders.Add(order);

        var handler = new ListSalesOrdersQueryHandler(repository);

        var result = await handler.Handle(
            new ListSalesOrdersQuery(
                Page: 2,
                PageSize: 5,
                Search: " so-9 ",
                Status: "confirmed",
                Sort: "-totalAmount"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalItems.Should().Be(1);

        var criteria = repository.LastCriteria;

        criteria.Should().NotBeNull();
        criteria!.Skip.Should().Be(5);
        criteria.Take.Should().Be(5);
        criteria.Search.Should().Be("so-9");
        criteria.Status.Should().Be(SalesOrderStatus.Confirmed);
        criteria.Sort.Should().ContainSingle();
        criteria.Sort[0].Field.Should().Be(SalesOrderSortField.TotalAmount);
        criteria.Sort[0].Descending.Should().BeTrue();
    }

    [Fact]
    public async Task List_WithInvalidStatus_ReturnsValidationError()
    {
        var repository = new FakeSalesOrderRepository();
        var handler = new ListSalesOrdersQueryHandler(repository);

        var result = await handler.Handle(
            new ListSalesOrdersQuery(Status: "Shipped"),
            CancellationToken.None);

        result.Error.Should().Be(SalesOrderErrors.InvalidStatusFilter);
        repository.LastCriteria.Should().BeNull();
    }

    [Fact]
    public async Task List_ClampsPageSizeToMaximum()
    {
        var repository = new FakeSalesOrderRepository();
        var handler = new ListSalesOrdersQueryHandler(repository);

        var result = await handler.Handle(
            new ListSalesOrdersQuery(Page: 0, PageSize: 1_000),
            CancellationToken.None);

        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(100);
        repository.LastCriteria!.Skip.Should().Be(0);
        repository.LastCriteria.Take.Should().Be(100);
    }

    [Fact]
    public async Task GetById_WhenOrderExists_ReturnsDto()
    {
        var repository = new FakeSalesOrderRepository();
        var order = CreateOrder(2, Now);
        repository.Orders.Add(order);

        var handler = new GetSalesOrderByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetSalesOrderByIdQuery(order.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.OrderNumber.Should().Be("SO-9002");
    }

    [Fact]
    public async Task GetById_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        var handler = new GetSalesOrderByIdQueryHandler(new FakeSalesOrderRepository());

        var result = await handler.Handle(
            new GetSalesOrderByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.Error.Should().Be(SalesOrderErrors.NotFound);
    }

    private static SalesOrder CreateOrder(int sequence, DateTimeOffset createdAt)
    {
        var orderId = Guid.Parse($"0192f5b4-8a00-7000-8000-{sequence:D12}");
        var lineId = Guid.Parse($"0192f5b4-8a00-7000-8000-{sequence + 100:D12}");
        var itemId = Guid.Parse($"0192f5b4-8a00-7000-8000-{sequence + 200:D12}");

        return SalesOrder.Create(
            orderId,
            TenantId,
            $"SO-{9000 + sequence}",
            CustomerId,
            [new SalesOrderLineInput(lineId, itemId, 1m, sequence * 10m)],
            createdAt,
            UserId).Value;
    }
}
