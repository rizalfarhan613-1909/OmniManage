using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.AssetMaintenance.Contracts.IntegrationEvents;
using OmniManage.Modules.Finance.Domain.Abstractions;

namespace OmniManage.Modules.Finance.Application.IntegrationEventHandlers;

/// <summary>
/// Consumes <see cref="AssetMaintenanceRequestedIntegrationEvent"/> and records
/// the maintenance request as a finance tracking accrual reference. FIN_GL is a
/// soft dependency of ASSET_CORE: when it is OFF the event is recorded as
/// SKIPPED_MODULE_DISABLED and the maintenance request stays open.
/// </summary>
/// <remarks>
/// The event carries no monetary amount, so no journal entry is posted until a
/// maintenance cost is confirmed. The accrual reference is emitted as a
/// structured log for financial tracking.
/// </remarks>
public sealed class AssetMaintenanceRequestedIntegrationEventHandler(
    IFinanceInbox inbox,
    IFinanceUnitOfWork unitOfWork,
    IModuleLicenseChecker licenses,
    TimeProvider clock,
    ILogger<AssetMaintenanceRequestedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<AssetMaintenanceRequestedIntegrationEvent>
{
    private const string EventTypeName = nameof(AssetMaintenanceRequestedIntegrationEvent);

    public string RequiredModuleCode => ModuleCodes.FinGl;

    private string ConsumerName => GetType().FullName ?? GetType().Name;

    public async Task HandleAsync(
        AssetMaintenanceRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var now = clock.GetUtcNow();

        if (await inbox
                .IsProcessedAsync(integrationEvent.EventId, ConsumerName, cancellationToken)
                .ConfigureAwait(false))
        {
            return;
        }

        if (!await licenses
                .IsActiveAsync(integrationEvent.TenantId, RequiredModuleCode, cancellationToken)
                .ConfigureAwait(false))
        {
            await MarkSkippedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Skipped integration event {EventId} of type {EventType} for consumer {Consumer}: module {ModuleCode} is disabled",
                integrationEvent.EventId,
                EventTypeName,
                ConsumerName,
                RequiredModuleCode);

            return;
        }

        await MarkProcessedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // No amount is available on the event yet, so this is a tracking record
        // rather than a posted accrual. Log identifiers only: descriptions may
        // contain free-text operational details.
        logger.LogInformation(
            "Recorded maintenance accrual reference for finance tracking: event {EventId}, work order {WorkOrderId}, asset {AssetId} ({AssetCode}), type {MaintenanceType}, priority {Priority}",
            integrationEvent.EventId,
            integrationEvent.WorkOrderId,
            integrationEvent.AssetId,
            integrationEvent.AssetCode,
            integrationEvent.MaintenanceType,
            integrationEvent.Priority);

        logger.LogInformation(
            "Processed integration event {EventId} of type {EventType} for consumer {Consumer}",
            integrationEvent.EventId,
            EventTypeName,
            ConsumerName);
    }

    private Task MarkProcessedAsync(
        AssetMaintenanceRequestedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkProcessedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);

    private Task MarkSkippedAsync(
        AssetMaintenanceRequestedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        inbox.MarkSkippedAsync(
            integrationEvent.EventId,
            ConsumerName,
            integrationEvent.TenantId,
            now,
            cancellationToken);
}
