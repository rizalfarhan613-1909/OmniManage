using FluentAssertions;
using OmniManage.Modules.Sales.Application.Orders.CreateSalesOrder;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.UnitTests.TestDoubles;

namespace OmniManage.Modules.Sales.UnitTests.Application;

public sealed class CreateSalesOrderCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000811");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000812");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000813");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000814");
    private static readonly Guid LineId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000815");
    private static readonly Guid OrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000816");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAndReturnsDto()
    {
        var repository = new FakeSalesOrderRepository();
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateSalesOrderCommand(
                " so-2001 ",
                CustomerId,
                [new CreateSalesOrderLine(ItemId, 2m, 10.25m)]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderNumber.Should().Be("SO-2001");
        result.Value.Status.Should().Be("Draft");
        result.Value.TotalAmount.Should().Be("20.5000");
        result.Value.Lines.Should().ContainSingle().Which.ItemId.Should().Be(ItemId);

        repository.Orders.Should().ContainSingle();
        repository.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenOrderNumberAlreadyExists_ReturnsConflictWithoutSaving()
    {
        var repository = new FakeSalesOrderRepository { OrderNumberExists = true };
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateSalesOrderCommand(
                "SO-2001",
                CustomerId,
                [new CreateSalesOrderLine(ItemId, 1m, 1m)]),
            CancellationToken.None);

        result.Error.Should().Be(SalesOrderErrors.OrderNumberAlreadyExists);
        repository.Orders.Should().BeEmpty();
        repository.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithoutLines_ReturnsNoLinesWithoutSaving()
    {
        var repository = new FakeSalesOrderRepository();
        var handler = CreateHandler(repository);

        var result = await handler.Handle(
            new CreateSalesOrderCommand("SO-2001", CustomerId, null),
            CancellationToken.None);

        result.Error.Should().Be(SalesOrderErrors.NoLines);
        repository.Orders.Should().BeEmpty();
        repository.SaveCount.Should().Be(0);
    }

    private static CreateSalesOrderCommandHandler CreateHandler(FakeSalesOrderRepository repository) =>
        new(
            repository,
            new TestTenantContext(TenantId),
            new FakeCurrentUser(UserId),
            new FixedGuidGenerator(LineId, OrderId),
            new FixedTimeProvider(Now));
}
