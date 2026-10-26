using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.Domain.Items;
using OmniManage.Modules.MasterData.Infrastructure.Persistence;
using OmniManage.Modules.MasterData.UnitTests.TestDoubles;

namespace OmniManage.Modules.MasterData.UnitTests.Persistence;

public sealed class MasterDataDbContextTests
{
    private static readonly Guid TenantA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000621");
    private static readonly Guid TenantB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000622");
    private static readonly Guid UserId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000623");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Model_MapsAggregatesToMasterDataSchema()
    {
        using var context = CreateContext(TenantA);

        AssertTable<BusinessPartner>(context, "business_partners");
        AssertTable<Item>(context, "items");
        AssertTable<OutboxMessage>(context, "outbox_messages");
        AssertTable<InboxMessage>(context, "inbox_messages");
    }

    [Fact]
    public void Model_ConfiguresTenantUniqueIndexes()
    {
        using var context = CreateContext(TenantA);

        var partnerIndex = context.Model
            .FindEntityType(typeof(BusinessPartner))!
            .GetIndexes()
            .Single(index => index.GetDatabaseName() == "uq_business_partners_tenant_id_code");

        partnerIndex.IsUnique.Should().BeTrue();
        partnerIndex.Properties.Select(property => property.Name)
            .Should().Equal(nameof(BusinessPartner.TenantId), nameof(BusinessPartner.Code));

        var itemIndex = context.Model
            .FindEntityType(typeof(Item))!
            .GetIndexes()
            .Single(index => index.GetDatabaseName() == "uq_items_tenant_id_sku");

        itemIndex.IsUnique.Should().BeTrue();
        itemIndex.Properties.Select(property => property.Name)
            .Should().Equal(nameof(Item.TenantId), nameof(Item.Sku));
    }

    [Fact]
    public void Model_AppliesTenantQueryFilterToAggregates()
    {
        using var context = CreateContext(TenantA);

        context.Model.FindEntityType(typeof(BusinessPartner))!.GetQueryFilter().Should().NotBeNull();
        context.Model.FindEntityType(typeof(Item))!.GetQueryFilter().Should().NotBeNull();
    }

    [Fact]
    public async Task Query_WhenTenantsShareDatabase_ReturnsOnlyCurrentTenantRows()
    {
        var options = CreateOptions($"omnimanage-masterdata-filter-{Guid.NewGuid():N}");
        var partnerA = PartnerId(1);
        var partnerB = PartnerId(2);

        await SeedPartnerAsync(options, TenantA, partnerA, "BP-A");
        await SeedPartnerAsync(options, TenantB, partnerB, "BP-B");

        await using var contextA = new MasterDataDbContext(options, new TestTenantContext(TenantA));
        var itemsA = await contextA.BusinessPartners.ToListAsync();

        itemsA.Should().ContainSingle();
        itemsA[0].Id.Should().Be(partnerA);
        itemsA[0].TenantId.Should().Be(TenantA);

        await using var contextB = new MasterDataDbContext(options, new TestTenantContext(TenantB));
        var itemsB = await contextB.BusinessPartners.ToListAsync();

        itemsB.Should().ContainSingle();
        itemsB[0].Id.Should().Be(partnerB);
        itemsB[0].TenantId.Should().Be(TenantB);
    }

    [Fact]
    public async Task SaveChanges_WhenAggregateBelongsToAnotherTenant_ThrowsTenantIsolationException()
    {
        var options = new DbContextOptionsBuilder<MasterDataDbContext>()
            .UseInMemoryDatabase($"omnimanage-masterdata-stamp-{Guid.NewGuid():N}")
            .AddTenantIsolation(new TestTenantContext(TenantA))
            .Options;

        await using var context = new MasterDataDbContext(options, new TestTenantContext(TenantA));

        context.BusinessPartners.Add(CreatePartner(TenantB, PartnerId(3), "BP-X"));

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    private static void AssertTable<TEntity>(MasterDataDbContext context, string expectedTable)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        entityType.Should().NotBeNull();
        entityType!.GetSchema().Should().Be(MasterDataDbContext.Schema);
        entityType!.GetTableName().Should().Be(expectedTable);
    }

    private static MasterDataDbContext CreateContext(Guid tenantId) =>
        new(CreateOptions($"omnimanage-masterdata-{Guid.NewGuid():N}"), new TestTenantContext(tenantId));

    private static DbContextOptions<MasterDataDbContext> CreateOptions(string databaseName) =>
        new DbContextOptionsBuilder<MasterDataDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    private static async Task SeedPartnerAsync(
        DbContextOptions<MasterDataDbContext> options,
        Guid tenantId,
        Guid partnerId,
        string code)
    {
        await using var context = new MasterDataDbContext(options, new TestTenantContext(tenantId));
        context.BusinessPartners.Add(CreatePartner(tenantId, partnerId, code));
        await context.SaveChangesAsync();
    }

    private static BusinessPartner CreatePartner(Guid tenantId, Guid partnerId, string code) =>
        BusinessPartner.Create(
            partnerId,
            tenantId,
            code,
            "Test Partner",
            "Customer",
            null,
            1000m,
            Now,
            UserId).Value;

    private static Guid PartnerId(int sequence) =>
        Guid.Parse($"0192f5b4-8a00-7000-8000-{sequence:D12}");
}
