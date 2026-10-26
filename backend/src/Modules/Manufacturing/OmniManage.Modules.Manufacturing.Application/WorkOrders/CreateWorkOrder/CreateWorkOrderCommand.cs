using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Manufacturing.Application.WorkOrders.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    string? OrderNumber,
    Guid BillOfMaterialId,
    decimal QuantityPlanned,
    DateOnly? DueDate,
    string? Notes) : ICommand<WorkOrderDto>;
