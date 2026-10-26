using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.AssetMaintenance.Application.IntegrationEvents;
using OmniManage.Modules.AssetMaintenance.Contracts.IntegrationEvents;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Errors;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Application.Maintenance.RequestMaintenance;

public sealed class RequestMaintenanceCommandHandler(
    IAssetRepository assets,
    IMaintenanceOrderRepository maintenanceOrders,
    IAssetMaintenanceOutbox outbox,
    IIntegrationEventDispatcher dispatcher,
    ITenantContext tenantContext,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<RequestMaintenanceCommandHandler> logger)
    : ICommandHandler<RequestMaintenanceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        RequestMaintenanceCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var asset = await assets
            .GetByIdAsync(command.AssetId, cancellationToken)
            .ConfigureAwait(false);

        if (asset is null)
        {
            return AssetErrors.NotFound;
        }

        var created = MaintenanceOrder.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.AssetId,
            command.RequestNumber,
            command.ScheduledDate,
            command.Description);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var maintenanceOrder = created.Value;

        if (await maintenanceOrders
                .RequestNumberExistsAsync(maintenanceOrder.RequestNumber, cancellationToken)
                .ConfigureAwait(false))
        {
            return MaintenanceOrderErrors.RequestNumberAlreadyExists;
        }

        var now = clock.GetUtcNow();

        var integrationEvent = new AssetMaintenanceRequestedIntegrationEvent(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            now.UtcDateTime,
            maintenanceOrder.Id,
            asset.Id,
            asset.AssetCode,
            Normalize(command.MaintenanceType, RequestMaintenanceCommand.DefaultMaintenanceType),
            Normalize(command.Priority, RequestMaintenanceCommand.DefaultPriority),
            maintenanceOrder.Description,
            now.UtcDateTime);

        // The maintenance order and the outbox row are committed together.
        await maintenanceOrders
            .AddAsync(maintenanceOrder, cancellationToken)
            .ConfigureAwait(false);
        await outbox
            .EnqueueAsync(integrationEvent, cancellationToken)
            .ConfigureAwait(false);
        await maintenanceOrders
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        await IntegrationEventDispatch
            .DispatchAfterCommitAsync(outbox, dispatcher, integrationEvent, logger, cancellationToken)
            .ConfigureAwait(false);

        return maintenanceOrder.Id;
    }

    private static string Normalize(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
