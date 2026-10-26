using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.UnitTests.TestDoubles;

namespace OmniManage.BuildingBlocks.UnitTests.Persistence;

public sealed class TenantSaveChangesInterceptorTests
{
    private static readonly Guid TenantA = Guid.Parse("0192f5b4-8a00-7000-8000-000000000201");
    private static readonly Guid TenantB = Guid.Parse("0192f5b4-8a00-7000-8000-000000000202");
    private static readonly Guid WidgetId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000203");

    [Fact]
    public async Task SaveChanges_WhenEntityTenantIsEmpty_StampsCurrentTenant()
    {
        var options = CreateOptions();
        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));
        var widget = new TenantWidget(WidgetId, Guid.Empty, "Widget");

        context.Widgets.Add(widget);
        await context.SaveChangesAsync();

        widget.TenantId.Should().Be(TenantA);
    }

    [Fact]
    public async Task SaveChanges_WhenAddedEntityTenantDiffers_ThrowsTenantIsolationException()
    {
        var options = CreateOptions();
        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));
        context.Widgets.Add(new TenantWidget(WidgetId, TenantB, "Widget"));

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    [Fact]
    public async Task SaveChanges_WhenModifiedEntityTenantMatches_SavesChanges()
    {
        var options = CreateOptions();
        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));
        context.Widgets.Add(new TenantWidget(WidgetId, TenantA, "Widget"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var widget = await context.Widgets.SingleAsync();
        widget.Name = "Updated widget";
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var updated = await context.Widgets.SingleAsync();
        updated.Name.Should().Be("Updated widget");
        updated.TenantId.Should().Be(TenantA);
    }

    [Fact]
    public async Task SaveChanges_WhenModifiedEntityTenantDiffers_ThrowsTenantIsolationException()
    {
        var options = CreateOptions();
        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));
        context.Widgets.Add(new TenantWidget(WidgetId, TenantA, "Widget"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var widget = await context.Widgets.SingleAsync();
        context.Entry(widget).Property(item => item.TenantId).CurrentValue = TenantB;

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    [Fact]
    public async Task SaveChanges_WhenDeletedEntityTenantDiffers_ThrowsTenantIsolationException()
    {
        var options = CreateOptions();
        await using var context = new TestTenantDbContext(options, new TestTenantContext(TenantA));
        var foreignWidget = new TenantWidget(WidgetId, TenantB, "Widget");

        context.Attach(foreignWidget);
        context.Widgets.Remove(foreignWidget);

        Func<Task> act = () => context.SaveChangesAsync();

        await act.Should().ThrowAsync<TenantIsolationException>();
    }

    private static DbContextOptions<TestTenantDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestTenantDbContext>()
            .UseInMemoryDatabase($"omnimanage-tenant-stamping-{Guid.NewGuid():N}")
            .AddTenantIsolation(new TestTenantContext(TenantA))
            .Options;
}
