using FluentAssertions;
using OmniManage.Modules.Inventory.Application.Stock.GetStockBalances;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.UnitTests.TestDoubles;

namespace OmniManage.Modules.Inventory.UnitTests.Application;

public sealed class GetStockBalancesQueryHandlerTests
{
    private static readonly Guid WarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000751");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000752");

    [Fact]
    public async Task Handle_ReturnsPagedBalancesAndPassesFilters()
    {
        var repository = new FakeStockLedgerRepository();
        repository.Balances.Add(new StockBalance(ItemId, WarehouseId, 12.5m));
        repository.Balances.Add(new StockBalance(Guid.NewGuid(), WarehouseId, -2m));

        var handler = new GetStockBalancesQueryHandler(repository);

        var result = await handler.Handle(
            new GetStockBalancesQuery(Page: 2, PageSize: 1, WarehouseId: WarehouseId, ItemId: ItemId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(1);
        result.Value.TotalItems.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].QuantityOnHand.Should().Be("-2.0000");

        repository.LastCriteria.Should().NotBeNull();
        repository.LastCriteria!.Skip.Should().Be(1);
        repository.LastCriteria.Take.Should().Be(1);
        repository.LastCriteria.WarehouseId.Should().Be(WarehouseId);
        repository.LastCriteria.ItemId.Should().Be(ItemId);
    }

    [Fact]
    public async Task Handle_NormalizesInvalidPagination()
    {
        var repository = new FakeStockLedgerRepository();
        var handler = new GetStockBalancesQueryHandler(repository);

        var result = await handler.Handle(
            new GetStockBalancesQuery(Page: -5, PageSize: 0),
            CancellationToken.None);

        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(1);
        repository.LastCriteria!.Skip.Should().Be(0);
        repository.LastCriteria.Take.Should().Be(1);
        repository.LastCriteria.WarehouseId.Should().BeNull();
        repository.LastCriteria.ItemId.Should().BeNull();
    }
}
