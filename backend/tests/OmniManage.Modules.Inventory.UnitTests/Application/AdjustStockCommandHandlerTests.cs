using FluentAssertions;
using OmniManage.Modules.Inventory.Application.Stock.AdjustStock;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.UnitTests.TestDoubles;

namespace OmniManage.Modules.Inventory.UnitTests.Application;

public sealed class AdjustStockCommandHandlerTests
{
    private static readonly Guid EntryId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000731");
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000732");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000733");
    private static readonly Guid WarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000734");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000735");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithActiveWarehouse_AppendsLedgerEntry()
    {
        var warehouses = new FakeWarehouseRepository();
        warehouses.Warehouses.Add(CreateWarehouse());
        var stockLedger = new FakeStockLedgerRepository();
        var handler = CreateHandler(warehouses, stockLedger);

        var result = await handler.Handle(
            new AdjustStockCommand(WarehouseId, ItemId, 25.5m, "receive", "GRN-001"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.QuantityDelta.Should().Be("25.5000");
        result.Value.MovementType.Should().Be("Receive");
        result.Value.OccurredAtUtc.Should().Be(Now);

        stockLedger.Entries.Should().ContainSingle();
        stockLedger.Entries[0].TenantId.Should().Be(TenantId);
        stockLedger.Entries[0].WarehouseId.Should().Be(WarehouseId);
        stockLedger.Entries[0].ItemId.Should().Be(ItemId);
        stockLedger.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_UsesProvidedOccurrenceTime()
    {
        var warehouses = new FakeWarehouseRepository();
        warehouses.Warehouses.Add(CreateWarehouse());
        var stockLedger = new FakeStockLedgerRepository();
        var handler = CreateHandler(warehouses, stockLedger);
        var occurredAt = Now.AddHours(-2);

        var result = await handler.Handle(
            new AdjustStockCommand(WarehouseId, ItemId, -3m, "Issue", "SO-001", occurredAt),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.QuantityDelta.Should().Be("-3.0000");
        result.Value.OccurredAtUtc.Should().Be(occurredAt);
    }

    [Fact]
    public async Task Handle_WhenWarehouseDoesNotExist_ReturnsNotFound()
    {
        var stockLedger = new FakeStockLedgerRepository();
        var handler = CreateHandler(new FakeWarehouseRepository(), stockLedger);

        var result = await handler.Handle(
            new AdjustStockCommand(WarehouseId, ItemId, 1m, "Receive", "GRN-001"),
            CancellationToken.None);

        result.Error.Should().Be(WarehouseErrors.NotFound);
        stockLedger.Entries.Should().BeEmpty();
        stockLedger.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenWarehouseIsInactive_ReturnsInactive()
    {
        var warehouses = new FakeWarehouseRepository();
        var warehouse = CreateWarehouse();
        warehouse.Deactivate(Now, UserId).IsSuccess.Should().BeTrue();
        warehouses.Warehouses.Add(warehouse);
        var stockLedger = new FakeStockLedgerRepository();
        var handler = CreateHandler(warehouses, stockLedger);

        var result = await handler.Handle(
            new AdjustStockCommand(WarehouseId, ItemId, 1m, "Receive", "GRN-001"),
            CancellationToken.None);

        result.Error.Should().Be(WarehouseErrors.Inactive);
        stockLedger.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenQuantityIsZero_ReturnsDomainValidationError()
    {
        var warehouses = new FakeWarehouseRepository();
        warehouses.Warehouses.Add(CreateWarehouse());
        var stockLedger = new FakeStockLedgerRepository();
        var handler = CreateHandler(warehouses, stockLedger);

        var result = await handler.Handle(
            new AdjustStockCommand(WarehouseId, ItemId, 0m, "Adjustment", "ADJ-001"),
            CancellationToken.None);

        result.Error.Should().Be(StockErrors.InvalidQuantityDelta);
        stockLedger.SaveCount.Should().Be(0);
    }

    private static AdjustStockCommandHandler CreateHandler(
        FakeWarehouseRepository warehouses,
        FakeStockLedgerRepository stockLedger) =>
        new(
            warehouses,
            stockLedger,
            new TestTenantContext(TenantId),
            new FixedGuidGenerator(EntryId),
            new FixedTimeProvider(Now));

    private static Warehouse CreateWarehouse() =>
        Warehouse.Create(WarehouseId, TenantId, "WH-1", "Main Warehouse", null, Now, UserId).Value;
}
