using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Errors;
using OmniManage.Modules.Inventory.Domain.Warehouses;

namespace OmniManage.Modules.Inventory.Application.Warehouses.CreateWarehouse;

public sealed class CreateWarehouseCommandHandler(
    IWarehouseRepository warehouses,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<CreateWarehouseCommand, WarehouseDto>
{
    public async Task<Result<WarehouseDto>> Handle(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = Warehouse.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.Code,
            command.Name,
            command.Address,
            clock.GetUtcNow(),
            currentUser.UserId);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var warehouse = created.Value;

        if (await warehouses.CodeExistsAsync(warehouse.Code, cancellationToken).ConfigureAwait(false))
        {
            return WarehouseErrors.CodeAlreadyExists;
        }

        await warehouses.AddAsync(warehouse, cancellationToken).ConfigureAwait(false);
        await warehouses.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return WarehouseDto.From(warehouse);
    }
}
