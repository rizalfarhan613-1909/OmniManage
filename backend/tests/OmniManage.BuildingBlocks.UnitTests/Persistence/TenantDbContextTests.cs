using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.UnitTests.TestDoubles;

namespace OmniManage.BuildingBlocks.UnitTests.Persistence;

public sealed class TenantDbContextTests
{
    private static readonly Guid TenantA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000101");
    private static readonly Guid TenantB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000102");
    private static readonly Guid WidgetA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000103");
    private static readonly Guid WidgetB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000104");

    [Fact]
    public async Task Query_WhenTenantContextSet_ReturnsOnlyOwnedRows()
    {
        var options = CreateOptions();
        await SeedAsync(options, TenantA, WidgetA, "Widget A");
        await SeedAsync(options, TenantB, WidgetB, "Widget B");

        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));

        var widgets = await context.Widgets.ToListAsync();

        widgets.Should().ContainSingle();
        widgets[0].TenantId.Should().Be(TenantA);
    }

    [Fact]
    public async Task Query_WhenRowBelongsToAnotherTenant_ReturnsNull()
    {
        var options = CreateOptions();
        await SeedAsync(options, TenantA, WidgetA, "Widget A");
        await SeedAsync(options, TenantB, WidgetB, "Widget B");

        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));

        var widget = await context.Widgets.SingleOrDefaultAsync(item => item.Id == WidgetB);

        widget.Should().BeNull();
    }

    [Fact]
    public async Task Count_WhenTenantContextSet_CountsOnlyOwnedRows()
    {
        var options = CreateOptions();
        await SeedAsync(options, TenantA, WidgetA, "Widget A");
        await SeedAsync(options, TenantB, WidgetB, "Widget B");

        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantB));

        var count = await context.Widgets.CountAsync();

        count.Should().Be(1);
    }

    private static DbContextOptions<TestTenantDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestTenantDbContext>()
            .UseInMemoryDatabase($"omnimanage-tenant-filter-{Guid.NewGuid():N}")
            .Options;

    private static async Task SeedAsync(
        DbContextOptions<TestTenantDbContext> options,
        Guid tenantId,
        Guid widgetId,
        string name)
    {
        await using var context = new TestTenantDbContext(options, new TestTenantContext(tenantId));
        context.Widgets.Add(new TenantWidget(widgetId, tenantId, name));
        await context.SaveChangesAsync();
    }
}
