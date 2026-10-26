using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.Domain.Stock;

namespace OmniManage.Modules.Inventory.Application.Stock.AdjustStock;

public sealed class AdjustStockCommandHandler(
    IWarehouseRepository warehouses,
    IStockLedgerRepository stockLedger,
    ITenantContext tenantContext,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<AdjustStockCommand, StockEntryDto>
{
    public async Task<Result<StockEntryDto>> Handle(
        AdjustStockCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var warehouse = await warehouses
            .GetByIdAsync(command.WarehouseId, cancellationToken)
            .ConfigureAwait(false);

        if (warehouse is null)
        {
            return WarehouseErrors.NotFound;
        }

        if (!warehouse.IsActive)
        {
            return WarehouseErrors.Inactive;
        }

        var created = StockLedger.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.WarehouseId,
            command.ItemId,
            command.QuantityDelta,
            command.MovementType,
            command.ReferenceNumber,
            command.OccurredAtUtc ?? clock.GetUtcNow());

        if (created.IsFailure)
        {
            return created.Error;
        }

        var entry = created.Value;

        await stockLedger.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        await stockLedger.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return StockEntryDto.From(entry);
    }
}
