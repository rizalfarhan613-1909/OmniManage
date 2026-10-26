using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.AssetMaintenance.Contracts.IntegrationEvents;
using OmniManage.Modules.Finance.Application.IntegrationEventHandlers;
using OmniManage.Modules.Finance.UnitTests.TestDoubles;

namespace OmniManage.Modules.Finance.UnitTests.Application;

public sealed class AssetMaintenanceRequestedIntegrationEventHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000951");
    private static readonly Guid WorkOrderId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000952");
    private static readonly Guid AssetId = Guid.Parse("0192f5b4-8a00-7000-8000-000000000953");
    private static readonly DateTimeOffset Now = new(2026, 10, 22, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenGlIsActive_RecordsAccrualReferenceAndMarksProcessed()
    {
        var eventId = Guid.NewGuid();
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var licenses = CreateLicenseChecker(ModuleCodes.FinGl);

        var handler = CreateHandler(inbox, unitOfWork, licenses);

        await handler.HandleAsync(CreateEvent(eventId), CancellationToken.None);

        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].EventId.Should().Be(eventId);
        inbox.Entries[0].Status.Should().Be("Processed");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal((TenantId, ModuleCodes.FinGl));
    }

    [Fact]
    public async Task Handle_WhenGlModuleIsDisabled_MarksSkipped()
    {
        var inbox = new FakeFinanceInbox();
        var unitOfWork = new FakeFinanceUnitOfWork();
        var licenses = CreateLicenseChecker();

        var handler = CreateHandler(inbox, unitOfWork, licenses);

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        inbox.Entries.Should().ContainSingle();
        inbox.Entries[0].Status.Should().Be("SkippedModuleDisabled");
        unitOfWork.SaveCount.Should().Be(1);
        licenses.Checks.Should().Equal((TenantId, ModuleCodes.FinGl));
    }

    [Fact]
    public async Task Handle_WhenEventWasAlreadyProcessed_DoesNothing()
    {
        var inbox = new FakeFinanceInbox { AlreadyProcessed = true };
        var unitOfWork = new FakeFinanceUnitOfWork();
        var licenses = CreateLicenseChecker();

        var handler = CreateHandler(inbox, unitOfWork, licenses);

        await handler.HandleAsync(CreateEvent(Guid.NewGuid()), CancellationToken.None);

        inbox.Entries.Should().BeEmpty();
        unitOfWork.SaveCount.Should().Be(0);
        licenses.Checks.Should().BeEmpty();
    }

    private static FakeModuleLicenseChecker CreateLicenseChecker(params string[] activeModules)
    {
        var checker = new FakeModuleLicenseChecker();
        checker.ActiveModules.Clear();

        foreach (var moduleCode in activeModules)
        {
            checker.ActiveModules.Add(moduleCode);
        }

        return checker;
    }

    private static AssetMaintenanceRequestedIntegrationEventHandler CreateHandler(
        FakeFinanceInbox inbox,
        FakeFinanceUnitOfWork unitOfWork,
        FakeModuleLicenseChecker licenses) =>
        new(
            inbox,
            unitOfWork,
            licenses,
            new FixedTimeProvider(Now),
            NullLogger<AssetMaintenanceRequestedIntegrationEventHandler>.Instance);

    private static AssetMaintenanceRequestedIntegrationEvent CreateEvent(Guid eventId) =>
        new(
            eventId,
            TenantId,
            Now.UtcDateTime,
            WorkOrderId,
            AssetId,
            "AST-001",
            "Preventive",
            "High",
            "Quarterly service",
            Now.UtcDateTime);
}
