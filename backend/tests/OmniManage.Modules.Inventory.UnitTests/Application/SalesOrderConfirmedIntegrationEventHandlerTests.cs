using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OmniManage.Modules.Inventory.Application.IntegrationEventHandlers;
using OmniManage.Modules.Inventory.Domain.Stock;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.UnitTests.TestDoubles;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Inventory.UnitTests.Application;

public sealed class SalesOrderConfirmedIntegrationEventHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000851");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000852");
    private static readonly Guid WarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000853");
    private static readonly Guid SalesOrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000854");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000855");
    private static readonly Guid ItemOne = Guid.Parse("0192f5b4-8a00-7000-8000-000000000856");
    private static readonly Guid ItemTwo = Guid.Parse("0192f5b4-8a00-7000-8000-000000000857");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenModuleIsActive_IssuesStockAndMarksInboxProcessed()
    {
        var eventId = Guid.NewGuid();
        var stockLedger = new FakeStockLedgerRepository();
        var warehouses = CreateWarehouseRepository();
        var inbox = new FakeInventoryInbox();
        var licenses = new FakeModuleLicenseChecker();
        var handler = CreateHandler(stockLedger, warehouses, inbox, licenses);

        await handler.HandleAsync(CreateEvent(eventId), CancellationToken.None);

        stockLedger.Entries.Should().HaveCount(2);
        stockLedger.Entries.Should().OnlyContain(entry =>
            entry.MovementType == MovementType.Issue
            && entry.WarehouseId == WarehouseId
            && entry.ReferenceNumber.Contains(SalesOrderId.ToString("D")));
        stockLedger.Entries.Select(entry => entry.ItemId)
            .Should().BeEquivalentTo([ItemOne, ItemTwo]);
        stockLedger.Entries.Single(entry => entry.ItemId == ItemOne)
            .QuantityDelta.Should().Be(-3m);
        stockLedger.Entries.Single(entry => entry.ItemId == ItemTwo)
            .QuantityDelta.Should().Be(-1.5m);

        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].EventId.Should().Be(eventId);
        inbox.Entries[0].Status.Should().Be("Processed");
        stockLedger.SaveCount.Should().Be(1);
        licenses.Checks.Should().ContainSingle()
            .Which.ModuleCode.Should().Be("INV_CORE");
    }

    [Fact]
    public async Task Handle_WhenModuleIsDisabled_RecordsSkippedInboxWithoutIssuingStock()
    {
        var eventId = Guid.NewGuid();
        var stockLedger = new FakeStockLedgerRepository();
        var warehouses = CreateWarehouseRepository();
        var inbox = new FakeInventoryInbox();
        var licenses = new FakeModuleLicenseChecker { IsActive = false };
        var handler = CreateHandler(stockLedger, warehouses, inbox, licenses);

        await handler.HandleAsync(CreateEvent(eventId), CancellationToken.None);

        stockLedger.Entries.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("SkippedModuleDisabled");
        stockLedger.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenEventWasAlreadyProcessed_DoesNothing()
    {
        var stockLedger = new FakeStockLedgerRepository();
        var warehouses = CreateWarehouseRepository();
        var inbox = new FakeInventoryInbox { AlreadyProcessed = true };
        var licenses = new FakeModuleLicenseChecker { IsActive = false };
        var handler = CreateHandler(stockLedger, warehouses, inbox, licenses);

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        stockLedger.Entries.Should().BeEmpty();
        inbox.Entries.Should().BeEmpty();
        stockLedger.SaveCount.Should().Be(0);
        licenses.Checks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenTenantHasNoActiveWarehouse_MarksFailedAndThrows()
    {
        var stockLedger = new FakeStockLedgerRepository();
        var warehouses = new FakeWarehouseRepository();
        var inbox = new FakeInventoryInbox();
        var licenses = new FakeModuleLicenseChecker();
        var handler = CreateHandler(stockLedger, warehouses, inbox, licenses);

        Func<Task> act = () => handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no active warehouse*");
        stockLedger.Entries.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("Failed");
        stockLedger.SaveCount.Should().Be(1);
        warehouses.LastCriteria!.IsActive.Should().BeTrue();
        warehouses.LastCriteria.Take.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenEventHasNoLines_MarksFailedAndThrows()
    {
        var stockLedger = new FakeStockLedgerRepository();
        var warehouses = CreateWarehouseRepository();
        var inbox = new FakeInventoryInbox();
        var licenses = new FakeModuleLicenseChecker();
        var handler = CreateHandler(stockLedger, warehouses, inbox, licenses);

        var integrationEvent = new SalesOrderConfirmedIntegrationEvent(
            Guid.NewGuid(),
            TenantId,
            Now.UtcDateTime,
            SalesOrderId,
            CustomerId,
            []);

        Func<Task> act = () => handler.HandleAsync(integrationEvent, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*at least one line*");
        stockLedger.Entries.Should().BeEmpty();
        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("Failed");
    }

    private static SalesOrderConfirmedIntegrationEventHandler CreateHandler(
        FakeStockLedgerRepository stockLedger,
        FakeWarehouseRepository warehouses,
        FakeInventoryInbox inbox,
        FakeModuleLicenseChecker licenses) =>
        new(
            stockLedger,
            warehouses,
            inbox,
            licenses,
            new FixedGuidGenerator(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            new FixedTimeProvider(Now),
            NullLogger<SalesOrderConfirmedIntegrationEventHandler>.Instance);

    private static FakeWarehouseRepository CreateWarehouseRepository()
    {
        var repository = new FakeWarehouseRepository();
        repository.Warehouses.Add(
            Warehouse.Create(
                WarehouseId,
                TenantId,
                "WH-001",
                "Main Warehouse",
                null,
                Now,
                UserId).Value);

        return repository;
    }

    private static SalesOrderConfirmedIntegrationEvent CreateEvent(Guid eventId) =>
        new(
            eventId,
            TenantId,
            Now.UtcDateTime,
            SalesOrderId,
            CustomerId,
            [
                new SalesOrderConfirmedLine(ItemOne, 3m, 10m, 30m),
                new SalesOrderConfirmedLine(ItemTwo, 1.5m, 4m, 6m),
            ]);
}
