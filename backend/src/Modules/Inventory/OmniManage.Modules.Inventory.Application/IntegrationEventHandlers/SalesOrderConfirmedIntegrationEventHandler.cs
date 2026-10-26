using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Stock;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;

namespace OmniManage.Modules.Inventory.Application.IntegrationEventHandlers;

/// <summary>
/// Consumes <see cref="SalesOrderConfirmedIntegrationEvent"/> and deducts stock
/// immediately. INV_CORE is a soft dependency of SALES_CORE: when it is OFF the
/// event is recorded as SKIPPED_MODULE_DISABLED and the order stays confirmed.
/// </summary>
public sealed class SalesOrderConfirmedIntegrationEventHandler(
    IStockLedgerRepository stockLedger,
    IWarehouseRepository warehouses,
    IInventoryInbox inbox,
    IModuleLicenseChecker licenses,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<SalesOrderConfirmedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<SalesOrderConfirmedIntegrationEvent>
{
    public string RequiredModuleCode => ModuleCodes.InvCore;

    private string ConsumerName => GetType().FullName ?? GetType().Name;

    public async Task HandleAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
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
            await inbox
                .MarkSkippedAsync(
                    integrationEvent.EventId,
                    ConsumerName,
                    integrationEvent.TenantId,
                    now,
                    cancellationToken)
                .ConfigureAwait(false);
            await stockLedger.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Skipped integration event {EventId} of type {EventType} for consumer {Consumer}: module {ModuleCode} is disabled",
                integrationEvent.EventId,
                nameof(SalesOrderConfirmedIntegrationEvent),
                ConsumerName,
                RequiredModuleCode);
            return;
        }

        if (integrationEvent.Lines.Count == 0)
        {
            await MarkFailedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("A confirmed sales order event must contain at least one line.");
        }

        var (activeWarehouses, _) = await warehouses
            .ListAsync(
                new WarehouseListCriteria(
                    0,
                    1,
                    null,
                    true,
                    [new WarehouseSortOption(WarehouseSortField.Code, false)]),
                cancellationToken)
            .ConfigureAwait(false);

        var warehouse = activeWarehouses.FirstOrDefault();

        if (warehouse is null)
        {
            await MarkFailedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(
                "Stock cannot be issued because the tenant has no active warehouse.");
        }

        foreach (var line in integrationEvent.Lines)
        {
            var created = StockLedger.Create(
                guidGenerator.NewGuid(),
                integrationEvent.TenantId,
                warehouse.Id,
                line.ItemId,
                -line.Quantity,
                nameof(MovementType.Issue),
                $"SO-{integrationEvent.SalesOrderId:D}",
                now);

            if (created.IsFailure)
            {
                await MarkFailedAsync(integrationEvent, now, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException(created.Error.Message);
            }

            await stockLedger.AddAsync(created.Value, cancellationToken).ConfigureAwait(false);
        }

        await inbox
            .MarkProcessedAsync(
                integrationEvent.EventId,
                ConsumerName,
                integrationEvent.TenantId,
                now,
                cancellationToken)
            .ConfigureAwait(false);
        await stockLedger.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation(
            "Processed integration event {EventId} of type {EventType} for consumer {Consumer}",
            integrationEvent.EventId,
            nameof(SalesOrderConfirmedIntegrationEvent),
            ConsumerName);
    }

    private async Task MarkFailedAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await inbox
            .MarkFailedAsync(
                integrationEvent.EventId,
                ConsumerName,
                integrationEvent.TenantId,
                now,
                cancellationToken)
            .ConfigureAwait(false);
        await stockLedger.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
