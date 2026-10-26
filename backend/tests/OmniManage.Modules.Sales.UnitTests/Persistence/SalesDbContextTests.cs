using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Sales.Domain.Orders;
using OmniManage.Modules.Sales.Infrastructure.Persistence;
using OmniManage.Modules.Sales.UnitTests.TestDoubles;

namespace OmniManage.Modules.Sales.UnitTests.Persistence;

public sealed class SalesDbContextTests
{
    private static readonly Guid TenantA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000841");
    private static readonly Guid TenantB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000842");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000843");
    private static readonly Guid CustomerId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000844");
    private static readonly Guid ItemId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000845");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Model_MapsAggregatesToSalesSchema()
    {
        using var context = CreateContext(TenantA);

        AssertTable<SalesOrder>(context, "sales_orders");
        AssertTable<SalesOrderLine>(context, "sales_order_lines");
        AssertTable<OutboxMessage>(context, "outbox_messages");
        AssertTable<InboxMessage>(context, "inbox_messages");
    }

    [Fact]
    public void Model_ConfiguresTenantUniqueOrderNumberIndex()
    {
        using var context = CreateContext(TenantA);

        var index = context.Model
            .FindEntityType(typeof(SalesOrder))!
            .GetIndexes()
            .Single(orderIndex => orderIndex.GetDatabaseName() == "uq_sales_orders_tenant_id_order_number");

        index.IsUnique.Should().BeTrue();
        index.Properties.Select(property => property.Name)
            .Should().Equal(nameof(SalesOrder.TenantId), nameof(SalesOrder.OrderNumber));
    }

    [Fact]
    public void Model_AppliesTenantQueryFilter()
    {
        using var context = CreateContext(TenantA);

        context.Model.FindEntityType(typeof(SalesOrder))!.GetQueryFilter().Should().NotBeNull();
    }

    [Fact]
    public async Task Query_WhenTenantsShareDatabase_ReturnsOnlyCurrentTenantOrders()
    {
        var options = CreateOptions($"omnimanage-sales-filter-{Guid.NewGuid():N}");
        var orderA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000846");
        var orderB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000847");

        await SeedOrderAsync(options, TenantA, orderA, "SO-A");
        await SeedOrderAsync(options, TenantB, orderB, "SO-B");

        await using var contextA = new SalesDbContext(options, new TestTenantContext(TenantA));
        var ordersA = await contextA.SalesOrders.ToListAsync();

        ordersA.Should().ContainSingle();
        ordersA[0].Id.Should().Be(orderA);

        await using var contextB = new SalesDbContext(options, new TestTenantContext(TenantB));
        var ordersB = await contextB.SalesOrders.ToListAsync();

        ordersB.Should().ContainSingle();
        ordersB[0].Id.Should().Be(orderB);
    }

    [Fact]
    public async Task SaveChanges_WhenAggregateBelongsToAnotherTenant_ThrowsTenantIsolationException()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase($"omnimanage-sales-stamp-{Guid.NewGuid():N}")
            .AddTenantIsolation(new TestTenantContext(TenantA))
            .Options;

        await using var context = new SalesDbContext(options, new TestTenantContext(TenantA));

        context.SalesOrders.Add(CreateOrder(TenantB, Guid.NewGuid(), "SO-X"));

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    [Fact]
    public async Task SaveChanges_PersistsOrderAndLinesTogether()
    {
        var options = CreateOptions($"omnimanage-sales-persist-{Guid.NewGuid():N}");
        var orderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000848");

        await using (var context = new SalesDbContext(options, new TestTenantContext(TenantA)))
        {
            context.SalesOrders.Add(CreateOrder(TenantA, orderId, "SO-4001"));
            await context.SaveChangesAsync();
        }

        await using var verification = new SalesDbContext(options, new TestTenantContext(TenantA));
        var order = await verification.SalesOrders
            .Include(candidate => candidate.Lines)
            .SingleAsync(candidate => candidate.Id == orderId);

        order.OrderNumber.Should().Be("SO-4001");
        order.TotalAmount.Should().Be(25m);
        order.Lines.Should().ContainSingle();
        order.Lines[0].ItemId.Should().Be(ItemId);
    }

    private static void AssertTable<TEntity>(SalesDbContext context, string expectedTable)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        entityType.Should().NotBeNull();
        var requiredEntityType = entityType!;
        requiredEntityType.GetSchema().Should().Be(SalesDbContext.Schema);
        requiredEntityType.GetTableName().Should().Be(expectedTable);
    }

    private static SalesDbContext CreateContext(Guid tenantId) =>
        new(CreateOptions($"omnimanage-sales-{Guid.NewGuid():N}"), new TestTenantContext(tenantId));

    private static DbContextOptions<SalesDbContext> CreateOptions(string databaseName) =>
        new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    private static async Task SeedOrderAsync(
        DbContextOptions<SalesDbContext> options,
        Guid tenantId,
        Guid orderId,
        string orderNumber)
    {
        await using var context = new SalesDbContext(options, new TestTenantContext(tenantId));
        context.SalesOrders.Add(CreateOrder(tenantId, orderId, orderNumber));
        await context.SaveChangesAsync();
    }

    private static SalesOrder CreateOrder(Guid tenantId, Guid orderId, string orderNumber) =>
        SalesOrder.Create(
            orderId,
            tenantId,
            orderNumber,
            CustomerId,
            [new SalesOrderLineInput(Guid.NewGuid(), ItemId, 2.5m, 10m)],
            Now,
            UserId).Value;
}
