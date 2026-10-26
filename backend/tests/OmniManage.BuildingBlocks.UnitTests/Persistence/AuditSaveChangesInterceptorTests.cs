using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Infrastructure;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.BuildingBlocks.UnitTests.TestDoubles;

namespace OmniManage.BuildingBlocks.UnitTests.Persistence;

public sealed class AuditSaveChangesInterceptorTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000801");
    private static readonly Guid ActorId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000802");
    private static readonly Guid WidgetId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000803");

    [Fact]
    public async Task SaveChanges_WhenAuditableEntityAdded_WritesCreatedAuditWithAfterData()
    {
        await using var context = CreateContext();

        context.Widgets.Add(new AuditedWidget(WidgetId, TenantId, "Widget", "top-secret"));
        await context.SaveChangesAsync();

        var log = await context.AuditLogs.SingleAsync();

        log.Action.Should().Be("audited_widget.created");
        log.EntityType.Should().Be(nameof(AuditedWidget));
        log.EntityId.Should().Be(WidgetId.ToString());
        log.TenantId.Should().Be(TenantId);
        log.ActorId.Should().Be(ActorId);
        log.ActorType.Should().Be("User");
        log.BeforeData.Should().BeNull();
        log.AfterData.Should().Contain("\"name\":\"Widget\"");
        log.AfterData.Should().Contain("\"sensitiveValue\":\"[REDACTED]\"");
        log.RowHash.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChanges_WhenAuditableEntityModified_WritesBeforeAndAfterDiff()
    {
        await using var context = CreateContext();
        context.Widgets.Add(new AuditedWidget(WidgetId, TenantId, "Original", "top-secret"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var widget = await context.Widgets.SingleAsync();
        widget.Name = "Renamed";
        await context.SaveChangesAsync();

        var log = await context.AuditLogs.SingleAsync(item => item.Action == "audited_widget.updated");

        log.BeforeData.Should().Contain("\"name\":\"Original\"");
        log.AfterData.Should().Contain("\"name\":\"Renamed\"");
        log.BeforeData.Should().NotContain("\"sensitiveValue\"");
        log.AfterData.Should().NotContain("\"sensitiveValue\"");
    }

    [Fact]
    public async Task SaveChanges_WhenAuditableEntityDeleted_WritesDeletedAuditWithBeforeData()
    {
        await using var context = CreateContext();
        var widget = new AuditedWidget(WidgetId, TenantId, "Widget", "top-secret");
        context.Widgets.Add(widget);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var stored = await context.Widgets.SingleAsync();
        context.Widgets.Remove(stored);
        await context.SaveChangesAsync();

        var log = await context.AuditLogs.SingleAsync(item => item.Action == "audited_widget.deleted");

        log.BeforeData.Should().Contain("\"name\":\"Widget\"");
        log.BeforeData.Should().Contain("\"sensitiveValue\":\"[REDACTED]\"");
        log.AfterData.Should().BeNull();
    }

    private static TestAuditDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestAuditDbContext>()
            .UseInMemoryDatabase($"omnimanage-audit-{Guid.NewGuid():N}")
            .AddInterceptors(new AuditSaveChangesInterceptor(
                new TestAuditContext { TenantId = TenantId, ActorId = ActorId },
                new GuidGenerator(),
                TimeProvider.System))
            .Options;

        return new TestAuditDbContext(options);
    }
}
