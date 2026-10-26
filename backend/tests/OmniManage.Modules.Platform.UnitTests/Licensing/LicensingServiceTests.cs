using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Domain.Tenancy;
using OmniManage.Modules.Platform.Infrastructure.Licensing;
using OmniManage.Modules.Platform.Infrastructure.Persistence;
using OmniManage.Modules.Platform.UnitTests.TestDoubles;

namespace OmniManage.Modules.Platform.UnitTests.Licensing;

public sealed class LicensingServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000401");
    private static readonly Guid ChangedBy = Guid.Parse("0192f5b4-8a00-7000-8000-000000000402");
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsync_ReturnsOnlyActiveUnexpiredModules()
    {
        await using var context = CreateContext();
        context.TenantModules.AddRange(
            TenantModule.CreateActive(TenantId, ModuleCodes.SalesCore, ChangedBy, "Active", Now.AddDays(-1)).Value,
            TenantModule.CreateActive(
                TenantId,
                ModuleCodes.FinAr,
                ChangedBy,
                "Expired",
                Now.AddDays(-10),
                Now.AddDays(-1)).Value,
            TenantModule.CreateInactive(TenantId, ModuleCodes.InvCore, ChangedBy, "Inactive").Value);
        await context.SaveChangesAsync();

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new LicensingService(context, cache, new FixedTimeProvider(Now));

        var snapshot = await service.GetAsync(TenantId);

        snapshot.ActiveModuleCodes.Should().Equal(ModuleCodes.SalesCore);
        (await service.IsActiveAsync(TenantId, ModuleCodes.SalesCore)).Should().BeTrue();
        (await service.IsActiveAsync(TenantId, ModuleCodes.FinAr)).Should().BeFalse();
        (await service.IsActiveAsync(TenantId, ModuleCodes.InvCore)).Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_CachesUntilInvalidated()
    {
        await using var context = CreateContext();
        context.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.SalesCore, ChangedBy, "Active", Now.AddDays(-1)).Value);
        await context.SaveChangesAsync();

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new LicensingService(context, cache, new FixedTimeProvider(Now));

        var initial = await service.GetAsync(TenantId);
        initial.ActiveModuleCodes.Should().Equal(ModuleCodes.SalesCore);

        context.TenantModules.Add(
            TenantModule.CreateActive(TenantId, ModuleCodes.FinGl, ChangedBy, "Added", Now.AddDays(-1)).Value);
        await context.SaveChangesAsync();

        var cached = await service.GetAsync(TenantId);
        cached.ActiveModuleCodes.Should().Equal(ModuleCodes.SalesCore);

        await service.InvalidateAsync(TenantId);

        var refreshed = await service.GetAsync(TenantId);
        refreshed.ActiveModuleCodes.Should().BeEquivalentTo([ModuleCodes.FinGl, ModuleCodes.SalesCore]);
    }

    [Fact]
    public async Task GetAsync_WhenTenantIdIsEmpty_ReturnsEmptySnapshot()
    {
        await using var context = CreateContext();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new LicensingService(context, cache, new FixedTimeProvider(Now));

        var snapshot = await service.GetAsync(Guid.Empty);

        snapshot.ActiveModuleCodes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_WhenLicenseSourceFails_ThrowsLicenseSourceUnavailable()
    {
        var context = CreateContext();
        await context.DisposeAsync();

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new LicensingService(context, cache, new FixedTimeProvider(Now));

        Func<Task> action = () => service.GetAsync(TenantId);

        await action.Should().ThrowAsync<LicenseSourceUnavailableException>();
    }

    private static PlatformDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"omnimanage-licensing-{Guid.NewGuid():N}")
            .Options);
}
