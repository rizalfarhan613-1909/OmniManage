using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Inventory.Domain.Stock;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;
using OmniManage.Modules.Inventory.UnitTests.TestDoubles;

namespace OmniManage.Modules.Inventory.UnitTests.Persistence;

public sealed class InventoryDbContextTests
{
    private static readonly Guid TenantA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000761");
    private static readonly Guid TenantB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000762");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000763");
    private static readonly Guid WarehouseId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000764");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000765");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Model_MapsAggregatesToInventorySchema()
    {
        using var context = CreateContext(TenantA);

        AssertTable<Warehouse>(context, "warehouses");
        AssertTable<StockLedger>(context, "stock_ledger");
        AssertTable<OutboxMessage>(context, "outbox_messages");
        AssertTable<InboxMessage>(context, "inbox_messages");
    }

    [Fact]
    public void Model_ConfiguresTenantUniqueWarehouseCodeIndex()
    {
        using var context = CreateContext(TenantA);

        var index = context.Model
            .FindEntityType(typeof(Warehouse))!
            .GetIndexes()
            .Single(warehouseIndex => warehouseIndex.GetDatabaseName() == "uq_warehouses_tenant_id_code");

        index.IsUnique.Should().BeTrue();
        index.Properties.Select(property => property.Name)
            .Should().Equal(nameof(Warehouse.TenantId), nameof(Warehouse.Code));
    }

    [Fact]
    public void Model_AppliesTenantQueryFilters()
    {
        using var context = CreateContext(TenantA);

        context.Model.FindEntityType(typeof(Warehouse))!.GetQueryFilter().Should().NotBeNull();
        context.Model.FindEntityType(typeof(StockLedger))!.GetQueryFilter().Should().NotBeNull();
    }

    [Fact]
    public async Task Query_WhenTenantsShareDatabase_ReturnsOnlyCurrentTenantRows()
    {
        var options = CreateOptions($"omnimanage-inventory-filter-{Guid.NewGuid():N}");
        var warehouseA = WarehouseId;
        var warehouseB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000766");

        await SeedWarehouseAsync(options, TenantA, warehouseA, "WH-A");
        await SeedWarehouseAsync(options, TenantB, warehouseB, "WH-B");

        await using var contextA = new InventoryDbContext(options, new TestTenantContext(TenantA));
        var warehousesA = await contextA.Warehouses.ToListAsync();

        warehousesA.Should().ContainSingle();
        warehousesA[0].Id.Should().Be(warehouseA);
        warehousesA[0].TenantId.Should().Be(TenantA);

        await using var contextB = new InventoryDbContext(options, new TestTenantContext(TenantB));
        var warehousesB = await contextB.Warehouses.ToListAsync();

        warehousesB.Should().ContainSingle();
        warehousesB[0].Id.Should().Be(warehouseB);
        warehousesB[0].TenantId.Should().Be(TenantB);
    }

    [Fact]
    public async Task SaveChanges_WhenAggregateBelongsToAnotherTenant_ThrowsTenantIsolationException()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase($"omnimanage-inventory-stamp-{Guid.NewGuid():N}")
            .AddTenantIsolation(new TestTenantContext(TenantA))
            .Options;

        await using var context = new InventoryDbContext(options, new TestTenantContext(TenantA));

        context.Warehouses.Add(CreateWarehouse(TenantB, Guid.NewGuid(), "WH-X"));

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    [Fact]
    public async Task SaveChanges_WhenLedgerEntryIsModified_ThrowsBecauseLedgerIsAppendOnly()
    {
        await using var context = CreateContext(TenantA);
        var entry = CreateEntry(TenantA);
        context.StockLedgerEntries.Add(entry);
        await context.SaveChangesAsync();

        context.Entry(entry).State = EntityState.Modified;

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task SaveChanges_WhenLedgerEntryIsDeleted_ThrowsBecauseLedgerIsAppendOnly()
    {
        await using var context = CreateContext(TenantA);
        var entry = CreateEntry(TenantA);
        context.StockLedgerEntries.Add(entry);
        await context.SaveChangesAsync();

        context.StockLedgerEntries.Remove(entry);

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    private static void AssertTable<TEntity>(InventoryDbContext context, string expectedTable)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        entityType.Should().NotBeNull();
        entityType!.GetSchema().Should().Be(InventoryDbContext.Schema);
        entityType!.GetTableName().Should().Be(expectedTable);
    }

    private static InventoryDbContext CreateContext(Guid tenantId) =>
        new(CreateOptions($"omnimanage-inventory-{Guid.NewGuid():N}"), new TestTenantContext(tenantId));

    private static DbContextOptions<InventoryDbContext> CreateOptions(string databaseName) =>
        new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    private static async Task SeedWarehouseAsync(
        DbContextOptions<InventoryDbContext> options,
        Guid tenantId,
        Guid warehouseId,
        string code)
    {
        await using var context = new InventoryDbContext(options, new TestTenantContext(tenantId));
        context.Warehouses.Add(CreateWarehouse(tenantId, warehouseId, code));
        await context.SaveChangesAsync();
    }

    private static Warehouse CreateWarehouse(Guid tenantId, Guid warehouseId, string code) =>
        Warehouse.Create(
            warehouseId,
            tenantId,
            code,
            "Test Warehouse",
            null,
            Now,
            UserId).Value;

    private static StockLedger CreateEntry(Guid tenantId) =>
        StockLedger.Create(
            Guid.NewGuid(),
            tenantId,
            WarehouseId,
            ItemId,
            10m,
            "Receive",
            "GRN-001",
            Now).Value;
}
